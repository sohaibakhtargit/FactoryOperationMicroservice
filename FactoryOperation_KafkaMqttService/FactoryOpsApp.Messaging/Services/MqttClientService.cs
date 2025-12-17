using FactoryOperation_KafkaMqttService.FactoryOpsApp.Messaging.Config;
using FactoryOperation_KafkaMqttService.FactoryOpsApp.Messaging.Interfaces;
using FactoryOperation_KafkaMqttService.FactoryOpsApp.Messaging.Models;
using Microsoft.Extensions.Options;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Protocol;
using System.Collections.Concurrent;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;

namespace FactoryOperation_KafkaMqttService.FactoryOpsApp.Messaging.Services
{
    public class MqttClientService : IMqttClientService, IAsyncDisposable
    {
        private readonly MqttSettings _settings;
        private readonly ILogger<MqttClientService> _logger;
        private readonly IMqttClient _client;
        private readonly MqttClientOptions _options;

        // Single gate for connect/disconnect activities
        private readonly SemaphoreSlim _connectLock = new(1, 1);

        // handler registry: topicFilter -> (subscriptionId -> handler)
        private readonly ConcurrentDictionary<string, ConcurrentDictionary<Guid, Func<MqttMessage, Task>>> _handlers
            = new(StringComparer.OrdinalIgnoreCase);

        // step 3: offline publish buffer
        private readonly ConcurrentQueue<PendingPublish> _offlineBuffer = new();
        private readonly int _offlineMax;
        private readonly bool _offlineEnabled;
        private readonly SemaphoreSlim _flushLock = new(1, 1);

        private sealed record PendingPublish(string Topic, byte[] Payload, int QoS, bool Retain);

        public bool IsConnected => _client?.IsConnected ?? false;

        // NEW: ensure only one reconnect loop runs and we don’t overlap connects
        private int _reconnectLoopRunning = 0;

        public MqttClientService(IOptions<MqttSettings> options, ILogger<MqttClientService> logger)
        {
            _settings = options?.Value ?? throw new ArgumentNullException(nameof(options));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _offlineEnabled = _settings.OfflineBuffer?.Enable ?? false;
            _offlineMax = Math.Max(1, _settings.OfflineBuffer?.MaxMessages ?? 10000);

            var factory = new MqttFactory();
            _client = factory.CreateMqttClient();

            var host = ParseHost(_settings.BrokerUrl);
            if (string.IsNullOrWhiteSpace(host))
                throw new ArgumentException($"Invalid MQTT broker URL: '{(_settings.BrokerUrl ?? "").Trim()}'", nameof(_settings.BrokerUrl));

            var builder = new MqttClientOptionsBuilder()
                .WithClientId(_settings.ClientId ?? $"factoryops-{Guid.NewGuid():N}")
                .WithTcpServer(host, _settings.BrokerPort)
                .WithCleanSession(_settings.CleanSession)
                .WithKeepAlivePeriod(TimeSpan.FromSeconds(_settings.KeepAlive));

            if (!string.IsNullOrEmpty(_settings.Username))
                builder.WithCredentials(_settings.Username, _settings.Password);

            // TLS / mTLS
            if (_settings.Tls?.Enable == true)
            {
                builder.WithTlsOptions(o =>
                {
                    // Enable TLS (method, not property)
                    o.UseTls(true);

                    // SSL protocol versions (method, not property assign)
                    if (_settings.Tls.AllowedTlsVersions is { Length: > 0 })
                    {
                        o.WithSslProtocols(MapTlsVersions(_settings.Tls.AllowedTlsVersions));
                    }

                    // Client certificate (mTLS) optional
                    if (_settings.Tls.UseMutualTls &&
                        !string.IsNullOrEmpty(_settings.Tls.ClientCertPath) &&
                        File.Exists(_settings.Tls.ClientCertPath))
                    {
                        try
                        {
                            var clientCert = new X509Certificate2(_settings.Tls.ClientCertPath);
                            o.WithCertificates(new List<X509Certificate> { clientCert });
                            // Enforce trusted certificates (do not allow untrusted)
                            o.WithAllowUntrustedCertificates(false);
                            // If you need custom validation, set a handler. Returning true accepts the server cert.
                            o.WithCertificateValidationHandler(_ =>
                            {
                                // Keep strict by default; flip to true only when you fully manage trust chain.
                                return true;
                            });
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Failed to load MQTT client certificate from {Path}", _settings.Tls.ClientCertPath);
                        }
                    }

                    // CA/trust store: MQTTnet does not expose a direct TrustChain API across all platforms.
                    // Typically, system trust store is used. If you must pin a CA, rely on CertificateValidationHandler above
                    // and verify issuer/subject/thumbprint in that handler.
                    // Example (optional, stricter):
                    // o.WithCertificateValidationHandler(ctx =>
                    // {
                    //     var cert = ctx.Certificate;
                    //     return cert?.Issuer?.Contains("Your CA") == true;
                    // });
                });
            }

            // Last Will and Testament
            if (_settings.Lwt is not null)
            {
                var will = new MqttApplicationMessageBuilder()
                    .WithTopic(_settings.Lwt.Topic)
                    .WithPayload(_settings.Lwt.Payload ?? string.Empty)
                    .WithQualityOfServiceLevel((MqttQualityOfServiceLevel)_settings.Lwt.Qos)
                    .WithRetainFlag(_settings.Lwt.Retain)
                    .Build();

                builder.WithWillMessage(will);
            }

            _options = builder.Build();

            _client.ConnectedAsync += OnConnectedAsync;
            _client.DisconnectedAsync += OnDisconnectedAsync;
            _client.ApplicationMessageReceivedAsync += OnMessageReceivedAsync;
        }

        private Task OnConnectedAsync(MqttClientConnectedEventArgs e)
        {
            _logger.LogInformation("✅ Connected to MQTT broker {Broker}", _settings.BrokerUrl);
            _ = ResubscribeAllAsync();
            // step 3: flush offline buffer after reconnect
            _ = FlushOfflineBufferAsync();
            return Task.CompletedTask;
        }

        private async Task ResubscribeAllAsync()
        {
            try
            {
                var qos = (MqttQualityOfServiceLevel)Math.Clamp(_settings.QoS, 0, 2);

                // Take a snapshot of current filters
                var filters = _handlers.Keys.ToList();

                foreach (var filter in filters)
                {
                    var attempts = 0;
                    var maxAttempts = 3;
                    var delay = TimeSpan.FromMilliseconds(300);

                    while (attempts < maxAttempts)
                    {
                        try
                        {
                            var topicFilter = new MqttTopicFilterBuilder()
                                .WithTopic(filter)
                                .WithQualityOfServiceLevel(qos)
                                .Build();

                            var result = await _client.SubscribeAsync(topicFilter).ConfigureAwait(false);

                            var bad = result?.Items?.FirstOrDefault(i =>
                                i.ResultCode != MqttClientSubscribeResultCode.GrantedQoS0 &&
                                i.ResultCode != MqttClientSubscribeResultCode.GrantedQoS1 &&
                                i.ResultCode != MqttClientSubscribeResultCode.GrantedQoS2);

                            if (bad == null)
                            {
                                _logger.LogInformation("Resubscribed to {Topic}", filter);
                                break;
                            }

                            attempts++;
                            _logger.LogWarning("Resubscribe to {Topic} returned {Code} (attempt {Attempt}/{Max})",
                                filter, bad.ResultCode, attempts, maxAttempts);
                            await Task.Delay(delay).ConfigureAwait(false);
                            delay = TimeSpan.FromMilliseconds(Math.Min(delay.TotalMilliseconds * 2, 5000));
                        }
                        catch (Exception ex)
                        {
                            attempts++;
                            _logger.LogWarning(ex, "Failed to resubscribe to {Topic} (attempt {Attempt}/{Max})",
                                filter, attempts, maxAttempts);
                            await Task.Delay(delay).ConfigureAwait(false);
                            delay = TimeSpan.FromMilliseconds(Math.Min(delay.TotalMilliseconds * 2, 5000));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error during resubscribe all");
            }
        }

        private async Task FlushOfflineBufferAsync()
        {
            if (!_offlineEnabled) return;
            if (!_client.IsConnected) return;

            var taken = false;
            try
            {
                await _flushLock.WaitAsync().ConfigureAwait(false);
                taken = true;

                var drained = 0;
                while (_client.IsConnected && _offlineBuffer.TryDequeue(out var p))
                {
                    try
                    {
                        var msg = new MqttApplicationMessageBuilder()
                            .WithTopic(p.Topic)
                            .WithPayload(p.Payload)
                            .WithQualityOfServiceLevel((MqttQualityOfServiceLevel)p.QoS)
                            .WithRetainFlag(p.Retain)
                            .Build();

                        await _client.PublishAsync(msg).ConfigureAwait(false);
                        drained++;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to flush buffered MQTT publish to {Topic}, will requeue", p.Topic);
                        _offlineBuffer.Enqueue(p);
                        break;
                    }
                }

                if (drained > 0)
                    _logger.LogInformation("Flushed {Count} buffered MQTT messages", drained);
            }
            finally
            {
                if (taken) _flushLock.Release();
            }
        }

        private Task OnDisconnectedAsync(MqttClientDisconnectedEventArgs e)
        {
            _logger.LogWarning("⚠️ Disconnected from MQTT broker: {Reason}", e.ReasonString);
            // Do not await here. Fire-and-forget a single reconnect loop.
            _ = TryReconnectWithBackoffAsync();
            return Task.CompletedTask;
        }

        private Task OnMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs e)
        {
            var topic = e.ApplicationMessage.Topic;
            var payload = e.ApplicationMessage.PayloadSegment.ToArray();
            var qos = (int)e.ApplicationMessage.QualityOfServiceLevel;
            var retain = e.ApplicationMessage.Retain;

            var msg = new MqttMessage
            {
                Topic = topic,
                Payload = payload,
                QoS = qos,
                Retain = retain,
                ReceivedAt = DateTimeOffset.UtcNow
            };

            foreach (var kv in _handlers)
            {
                var filter = kv.Key;
                if (TopicMatchesFilter(topic, filter))
                {
                    var handlersSnapshot = kv.Value.Values.ToList();
                    foreach (var handler in handlersSnapshot)
                    {
                        _ = Task.Run(async () =>
                        {
                            try { await handler(msg).ConfigureAwait(false); }
                            catch (Exception ex) { _logger.LogError(ex, "MQTT handler for filter {Filter} failed", filter); }
                        });
                    }
                }
            }

            return Task.CompletedTask;
        }

        public async Task ConnectAsync(CancellationToken ct = default)
        {
            var lockTaken = false;
            try
            {
                // Serialize all connect attempts
                await _connectLock.WaitAsync(ct).ConfigureAwait(false);
                lockTaken = true;

                if (_client.IsConnected)
                    return;

                // Attempt connect; if a previous connect/disconnect was pending,
                // we are the only one allowed to proceed because of the lock.
                var result = await _client.ConnectAsync(_options, ct).ConfigureAwait(false);
                _logger.LogInformation("MQTT connection result: {ResultCode}", result.ResultCode);
            }
            finally
            {
                if (lockTaken)
                {
                    try { _connectLock.Release(); } catch { }
                }
            }
        }

        // UPDATED: use the same lock + single loop guard to avoid overlapping connects
        private async Task TryReconnectWithBackoffAsync()
        {
            if (Interlocked.Exchange(ref _reconnectLoopRunning, 1) == 1)
            {
                // A reconnect loop is already running
                return;
            }

            try
            {
                var delay = TimeSpan.FromSeconds(_settings.ReconnectDelaySeconds > 0 ? _settings.ReconnectDelaySeconds : 5);

                for (int attempt = 1; attempt <= 10; attempt++)
                {
                    if (_client.IsConnected) break;

                    try
                    {
                        _logger.LogInformation("Reconnecting (attempt {Attempt}) after {Delay}s...", attempt, delay.TotalSeconds);
                        await Task.Delay(delay).ConfigureAwait(false);

                        // Reuse ConnectAsync so it’s serialized with any other caller
                        await ConnectAsync().ConfigureAwait(false);

                        if (_client.IsConnected)
                        {
                            _ = FlushOfflineBufferAsync();
                            break;
                        }
                    }
                    catch (OperationCanceledException) { throw; }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Reconnect attempt {Attempt} failed", attempt);
                        delay = TimeSpan.FromSeconds(Math.Min(delay.TotalSeconds * 2, 60));
                    }
                }
            }
            finally
            {
                Interlocked.Exchange(ref _reconnectLoopRunning, 0);
            }
        }

        public async Task SubscribeAsync(string topic, Func<MqttMessage, Task> handler, CancellationToken ct = default)
        {
            if (handler == null) throw new ArgumentNullException(nameof(handler));
            if (string.IsNullOrWhiteSpace(topic)) throw new ArgumentNullException(nameof(topic));

            if (!_client.IsConnected)
                await ConnectAsync(ct).ConfigureAwait(false);

            var handlersForFilter = _handlers.GetOrAdd(topic, _ => new ConcurrentDictionary<Guid, Func<MqttMessage, Task>>());
            var id = Guid.NewGuid();
            handlersForFilter.TryAdd(id, handler);

            if (handlersForFilter.Count == 1)
            {
                try
                {
                    var qos = (MqttQualityOfServiceLevel)Math.Clamp(_settings.QoS, 0, 2);

                    var topicFilter = new MqttTopicFilterBuilder()
                        .WithTopic(topic)
                        .WithQualityOfServiceLevel(qos)
                        .Build();

                    await _client.SubscribeAsync(topicFilter, ct).ConfigureAwait(false);
                    _logger.LogInformation("📡 Subscribed to topic: {Topic}", topic);
                }
                catch (Exception ex)
                {
                    handlersForFilter.TryRemove(id, out _);
                    if (handlersForFilter.IsEmpty)
                        _handlers.TryRemove(topic, out _);

                    _logger.LogError(ex, "Failed to subscribe to topic {Topic}", topic);
                    throw;
                }
            }
            else
            {
                _logger.LogDebug("Handler registered for topic {Topic} (total handlers={Count})", topic, handlersForFilter.Count);
            }
        }

        public async Task UnsubscribeAsync(string topic, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(topic)) return;

            _handlers.TryRemove(topic, out _);

            if (!_client.IsConnected)
                await ConnectAsync(ct).ConfigureAwait(false);

            try
            {
                await _client.UnsubscribeAsync(topic, ct).ConfigureAwait(false);
                _logger.LogInformation("Unsubscribed from topic: {Topic}", topic);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to unsubscribe from {Topic}", topic);
            }
        }

        public async Task PublishAsync(string topic, byte[] payload, int qos = 1, bool retain = false, CancellationToken ct = default)
        {
            // Best-effort connect
            if (!_client.IsConnected)
            {
                try
                {
                    await ConnectAsync(ct).ConfigureAwait(false);
                    await Task.Delay(200, ct).ConfigureAwait(false);
                }
                catch (OperationCanceledException) { throw; }
                catch
                {
                    // fall-through to buffer if enabled
                }
            }

            if (!_client.IsConnected)
            {
                if (_offlineEnabled)
                {
                    BufferOffline(topic, payload, qos, retain);
                    return;
                }

                _logger.LogWarning("PublishAsync: MQTT client not connected and offline buffer disabled; skipping publish to {Topic}", topic);
                return;
            }

            var message = new MqttApplicationMessageBuilder()
                .WithTopic(topic)
                .WithPayload(payload)
                .WithQualityOfServiceLevel((MqttQualityOfServiceLevel)qos)
                .WithRetainFlag(retain)
                .Build();

            try
            {
                var result = await _client.PublishAsync(message, ct).ConfigureAwait(false);
                _logger.LogDebug("📤 Published MQTT message to {Topic}, result {ResultCode}", topic, result.ReasonCode);
            }
            catch (MQTTnet.Exceptions.MqttClientNotConnectedException)
            {
                if (_offlineEnabled)
                {
                    BufferOffline(topic, payload, qos, retain);
                    return;
                }

                _logger.LogWarning("PublishAsync: client lost connection and offline buffer disabled; dropping message to {Topic}", topic);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "PublishAsync: error publishing to {Topic}", topic);
                if (_offlineEnabled)
                {
                    BufferOffline(topic, payload, qos, retain);
                }
            }
        }

        private void BufferOffline(string topic, byte[] payload, int qos, bool retain)
        {
            while (_offlineBuffer.Count >= _offlineMax && _offlineBuffer.TryDequeue(out _)) { }

            _offlineBuffer.Enqueue(new PendingPublish(topic, payload, qos, retain));
            _logger.LogInformation("Buffered offline MQTT publish for topic {Topic}. Buffered={Count}/{Max}", topic, _offlineBuffer.Count, _offlineMax);
        }

        public async Task DisconnectAsync(CancellationToken ct = default)
        {
            // Serialize disconnect through the same gate to avoid overlapping with ConnectAsync
            var lockTaken = false;
            try
            {
                await _connectLock.WaitAsync(ct).ConfigureAwait(false);
                lockTaken = true;

                if (_client.IsConnected)
                {
                    var options = new MqttClientDisconnectOptions
                    {
                        Reason = (MqttClientDisconnectOptionsReason)MqttClientDisconnectReason.NormalDisconnection
                    };

                    await _client.DisconnectAsync(options, ct).ConfigureAwait(false);
                    _logger.LogInformation("MQTT client disconnected gracefully.");
                }
            }
            finally
            {
                if (lockTaken)
                {
                    try { _connectLock.Release(); } catch { }
                }
            }
        }

        public async ValueTask DisposeAsync()
        {
            try
            {
                await DisconnectAsync().ConfigureAwait(false);
                _client.ConnectedAsync -= OnConnectedAsync;
                _client.DisconnectedAsync -= OnDisconnectedAsync;
                _client.ApplicationMessageReceivedAsync -= OnMessageReceivedAsync;

                _client?.Dispose();
                _connectLock?.Dispose();
                _flushLock?.Dispose();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error disposing MQTT client");
            }
        }

        private static bool TopicMatchesFilter(string topic, string filter)
        {
            if (topic == null || filter == null) return false;

            var tLevels = topic.Split('/');
            var fLevels = filter.Split('/');

            int ti = 0;
            for (int fi = 0; fi < fLevels.Length; fi++)
            {
                var f = fLevels[fi];

                if (f == "#")
                {
                    return true;
                }

                if (ti >= tLevels.Length)
                {
                    return false;
                }

                if (f == "+")
                {
                    ti++;
                    continue;
                }

                if (!string.Equals(f, tLevels[ti], StringComparison.Ordinal))
                {
                    return false;
                }

                ti++;
            }

            return ti == tLevels.Length;
        }

        private static SslProtocols MapTlsVersions(string[] versions)
        {
            var p = SslProtocols.None;
            foreach (var v in versions)
            {
                switch ((v ?? string.Empty).Trim().ToUpperInvariant())
                {
                    case "TLS12":
                    case "TLS1.2":
                        p |= SslProtocols.Tls12;
                        break;
                    case "TLS13":
                    case "TLS1.3":
                        p |= SslProtocols.Tls13;
                        break;
                }
            }
            return p == SslProtocols.None ? SslProtocols.Tls12 | SslProtocols.Tls13 : p;
        }

        private static string ParseHost(string url)
        {
            // keeps existing behavior; simple tcp://host:port or host string support
            if (string.IsNullOrWhiteSpace(url)) return string.Empty;
            if (url.Contains("://"))
            {
                try
                {
                    var u = new Uri(url);
                    return string.IsNullOrWhiteSpace(u.Host) ? url : u.Host;
                }
                catch { return url; }
            }
            return url;
        }
    }
}