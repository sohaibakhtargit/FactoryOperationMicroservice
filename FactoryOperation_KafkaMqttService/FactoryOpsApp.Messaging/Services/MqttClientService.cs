using FactoryOperation_KafkaMqttService.FactoryOpsApp.Messaging.Config;
using FactoryOperation_KafkaMqttService.FactoryOpsApp.Messaging.Interfaces;
using FactoryOperation_KafkaMqttService.FactoryOpsApp.Messaging.Models;
using FactoryOps.Shared.Observability;
using Microsoft.Extensions.Options;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Protocol;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;

namespace FactoryOperation_KafkaMqttService.FactoryOpsApp.Messaging.Services
{
    /// <summary>
    /// Production-hardened MQTT client.
    /// Backward compatible.
    /// Supports DB-driven config + hot reload (optional).
    /// </summary>
    public sealed class MqttClientService : IMqttClientService, IAsyncDisposable
    {
        private readonly ILogger<MqttClientService> _logger;
        private readonly IMqttClient _client;
        private readonly IMessagingSettingsProvider? _settingsProvider;

        private MqttSettings _settings = default!;
        private MqttClientOptions _options = default!;


        // =========================
        // Runtime Safety Settings
        // =========================
        private int _maxPayloadBytes;
        private bool _chunkEnabled;
        private int _chunkSizeBytes;
        private bool _offlineEnabled;
        private int _offlineMax;

        // =========================
        // Chunking / Buffers
        // =========================
        private readonly ConcurrentDictionary<string, SortedDictionary<int, byte[]>> _chunkBuffer = new();
        private readonly ConcurrentDictionary<string, DateTimeOffset> _chunkTimestamps = new();
        private readonly TimeSpan _chunkTtl = TimeSpan.FromMinutes(2);
        private readonly CancellationTokenSource _chunkCleanupCts = new();

        // =========================
        // Concurrency / Sync
        // =========================
        private readonly SemaphoreSlim _connectLock = new(1, 1);
        private readonly SemaphoreSlim _flushLock = new(1, 1);

        private readonly ConcurrentDictionary<string, ConcurrentDictionary<Guid, Func<MqttMessage, Task>>> _handlers
            = new(StringComparer.OrdinalIgnoreCase);

        private readonly ConcurrentQueue<PendingPublish> _offlineBuffer = new();

        private int _reconnectLoopRunning = 0;

        private sealed record PendingPublish(string Topic, byte[] Payload, int QoS, bool Retain);

        public bool IsConnected => _client?.IsConnected ?? false;

        // ======================================================
        // CONSTRUCTOR
        // ======================================================
        public MqttClientService(
            IOptions<MqttSettings> options,
            ILogger<MqttClientService> logger,
            IMessagingSettingsProvider? settingsProvider = null)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _settingsProvider = settingsProvider;

            _settings = options?.Value ?? throw new ArgumentNullException(nameof(options));

            var factory = new MqttFactory();
            _client = factory.CreateMqttClient();

            BuildRuntimeSettings(_settings);

            _client.ConnectedAsync += OnConnectedAsync;
            _client.DisconnectedAsync += OnDisconnectedAsync;
            _client.ApplicationMessageReceivedAsync += OnMessageReceivedAsync;

            if (_settingsProvider != null)
                _settingsProvider.SettingsChanged += OnSettingsChanged;

            _ = StartChunkCleanupLoopAsync(_chunkCleanupCts.Token);
        }

        // ======================================================
        // SETTINGS HOT RELOAD (SAFE)
        // ======================================================
        private void OnSettingsChanged(object? sender, int? tenantId)
        {
            try
            {
                if (_settingsProvider == null)
                    return;

                _logger.LogInformation("Reloading MQTT settings from DB (tenant={TenantId})", tenantId);

                var newSettings = _settingsProvider
                    .GetEffectiveSettings(tenantId)
                    .Mqtt;

                _settings = newSettings;
                BuildRuntimeSettings(_settings);

                _ = Task.Run(async () =>
                {
                    await DisconnectAsync();
                    await ConnectAsync();
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to reload MQTT settings");
            }
        }

        private void BuildRuntimeSettings(MqttSettings settings)
        {
            _maxPayloadBytes = settings.Publish?.MaxPayloadBytes > 0
                ? settings.Publish.MaxPayloadBytes
                : 1_048_576;

            _chunkEnabled = settings.Publish?.ChunkLargePayloads ?? true;

            _chunkSizeBytes = settings.Publish?.ChunkSizeBytes > 0
                ? settings.Publish.ChunkSizeBytes
                : 262_144;

            _offlineEnabled = settings.OfflineBuffer?.Enable ?? false;
            _offlineMax = Math.Max(1, settings.OfflineBuffer?.MaxMessages ?? 10_000);

            _options = BuildClientOptions(settings);
        }

        // ======================================================
        // MQTT OPTIONS
        // ======================================================
        private MqttClientOptions BuildClientOptions(MqttSettings settings)
        {
            var host = ParseHost(settings.BrokerUrl);
            if (string.IsNullOrWhiteSpace(host))
                throw new ArgumentException($"Invalid MQTT broker URL: '{settings.BrokerUrl}'");

            var cleanSession = settings.CleanSession;

            string? clientId = settings.ClientId;

            if (string.IsNullOrWhiteSpace(clientId) && !string.IsNullOrWhiteSpace(settings.ClientIdTemplate))
            {
                clientId = settings.ClientIdTemplate.Replace("{tenantId}", settings.TenantId ?? "1");
            }

            if (!cleanSession && string.IsNullOrWhiteSpace(clientId))
            {
                throw new InvalidOperationException("MQTT ClientId must be configured when CleanSession=false");
            }

            if (string.IsNullOrWhiteSpace(clientId))
            {
                throw new InvalidOperationException("MQTT ClientId is required after template resolution");
            }

            var builder = new MqttClientOptionsBuilder()
                .WithClientId(clientId)
                .WithTcpServer(host, settings.BrokerPort)
                .WithCleanSession(cleanSession)
                .WithKeepAlivePeriod(TimeSpan.FromSeconds(settings.KeepAlive));

            if (!string.IsNullOrEmpty(settings.Username))
                builder.WithCredentials(settings.Username, settings.Password);

            if (settings.Tls?.Enable == true)
            {
                builder.WithTlsOptions(tls =>
                {
                    tls.UseTls();

                    if (!string.IsNullOrWhiteSpace(settings.Tls.CaCertPath))
                    {
                        tls.WithCertificateValidationHandler(context =>
                        {
                            try
                            {
                                var caCert = new X509Certificate2(settings.Tls.CaCertPath);
                                return context.Chain?.ChainStatus.All(
                                    s => s.Status == X509ChainStatusFlags.NoError) == true;
                            }
                            catch { return false; }
                        });
                    }

                    if (settings.Tls.UseMutualTls == true &&
                        !string.IsNullOrWhiteSpace(settings.Tls.ClientCertPath))
                    {
                        tls.WithClientCertificates(new[]
                        {
                            new X509Certificate2(settings.Tls.ClientCertPath)
                        });
                    }
                });
            }

            return builder.Build();
        }

        private async Task OnConnectedAsync(MqttClientConnectedEventArgs e)
        {
            try
            {
                _logger.LogInformation("Connected to MQTT broker {Broker}", _settings.BrokerUrl);

                await _client.SubscribeAsync("test/keepalive");

                _logger.LogInformation("Subscribed to test/keepalive (health topic)");

                _ = Task.Run(async () =>
                {
                    while (_client.IsConnected)
                    {
                        try
                        {
                            var payload = Encoding.UTF8.GetBytes("alive");

                            await _client.PublishAsync(
                                new MqttApplicationMessageBuilder()
                                    .WithTopic("health/ping")
                                    .WithPayload(payload)
                                    .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
                                    .WithRetainFlag(false)
                                    .Build()
                            );

                            _logger.LogDebug("Heartbeat sent to health/ping");

                            await Task.Delay(5000);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Heartbeat failed");
                        }
                    }
                });

                _ = ResubscribeAllAsync();
                _ = FlushOfflineBufferAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inside OnConnectedAsync");
            }
        }

        private Task OnDisconnectedAsync(MqttClientDisconnectedEventArgs e)
        {
            _logger.LogWarning("Disconnected from MQTT broker: {Reason}", e.ReasonString);
            _ = TryReconnectWithBackoffAsync();
            return Task.CompletedTask;
        }

        private async Task OnMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs e)
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

            Telemetry.MqttMessagesReceived.Add(1);
            Telemetry.MqttBytesReceived.Add(payload.Length);

            using var activity = Telemetry.Activity.StartActivity(
                "mqtt.message.received",
                ActivityKind.Consumer);

            activity?.SetTag("mqtt.topic", topic);
            activity?.SetTag("mqtt.qos", qos);
            activity?.SetTag("mqtt.retain", retain);

            if (topic.EndsWith("/_chunk", StringComparison.OrdinalIgnoreCase))
            {
                await HandleChunkAsync(msg);
                return;
            }

            await DispatchAsync(msg);
        }

        // ======================================================
        // PUBLIC API
        // ======================================================
        public async Task ConnectAsync(CancellationToken ct = default)
        {
            var taken = false;
            try
            {
                await _connectLock.WaitAsync(ct);
                taken = true;

                if (_client.IsConnected)
                    return;

                _logger.LogWarning(
                    "MQTT CONNECT → {Host}:{Port} | ClientId={ClientId}",
                    _settings.BrokerUrl,
                    _settings.BrokerPort,
                    _settings.ClientId
                );


                var result = await _client.ConnectAsync(_options, ct);
                _logger.LogInformation("MQTT connection result: {ResultCode}", result.ResultCode);
            }
            finally
            {
                if (taken) _connectLock.Release();
            }
        }

        public async Task DisconnectAsync(CancellationToken ct = default)
        {
            var taken = false;
            try
            {
                await _connectLock.WaitAsync(ct);
                taken = true;

                if (_client.IsConnected)
                {
                    var options = new MqttClientDisconnectOptions
                    {
                        Reason = (MqttClientDisconnectOptionsReason)
                            MqttClientDisconnectReason.NormalDisconnection
                    };

                    await _client.DisconnectAsync(options, ct);
                    _logger.LogInformation("MQTT client disconnected gracefully.");
                }
            }
            finally
            {
                if (taken) _connectLock.Release();
            }
        }

        public async Task SubscribeAsync(string topic, Func<MqttMessage, Task> handler, CancellationToken ct = default)
        {
            if (!_client.IsConnected)
                await ConnectAsync(ct);

            var handlersForFilter = _handlers.GetOrAdd(topic,
                _ => new ConcurrentDictionary<Guid, Func<MqttMessage, Task>>());

            var id = Guid.NewGuid();
            handlersForFilter.TryAdd(id, handler);

            if (handlersForFilter.Count == 1)
            {
                var qos = (MqttQualityOfServiceLevel)Math.Clamp(_settings.QoS, 0, 2);

                var filter = new MqttTopicFilterBuilder()
                    .WithTopic(topic)
                    .WithQualityOfServiceLevel(qos)
                    .Build();

                await _client.SubscribeAsync(filter, ct);
                _logger.LogInformation("Subscribed to {Topic}", topic);
            }
        }

        public async Task UnsubscribeAsync(string topic, CancellationToken ct = default)
        {
            _handlers.TryRemove(topic, out _);

            if (_client.IsConnected)
                await _client.UnsubscribeAsync(topic, ct);
        }

        public async Task PublishAsync(
            string topic,
            byte[] payload,
            int qos = 1,
            bool retain = false,
            CancellationToken ct = default)
        {
            // Backpressure
            if (_offlineEnabled && _offlineBuffer.Count > _offlineMax * 0.9)
            {
                _logger.LogWarning("MQTT backpressure active — dropping publish for {Topic}", topic);
                return;
            }

            // HARD SAFETY LIMIT
            if (payload.Length > _maxPayloadBytes)
            {
                _logger.LogWarning(
                    "Payload exceeds MaxPayloadBytes ({Size}>{Max}) for {Topic}",
                    payload.Length,
                    _maxPayloadBytes,
                    topic);
                return;
            }

            // Chunking
            if (_chunkEnabled && payload.Length > _chunkSizeBytes)
            {
                await PublishChunkedAsync(topic, payload, qos, retain, ct);
                return;
            }

            await PublishInternalAsync(topic, payload, qos, retain, ct);
        }

        // ======================================================
        // INTERNAL HELPERS
        // ======================================================
        private async Task TryReconnectWithBackoffAsync()
        {
            if (Interlocked.Exchange(ref _reconnectLoopRunning, 1) == 1)
                return;

            try
            {
                var delay = TimeSpan.FromSeconds(5);

                for (int i = 1; i <= 10; i++)
                {
                    if (_client.IsConnected) break;
                    await Task.Delay(delay);
                    await ConnectAsync();
                    delay = TimeSpan.FromSeconds(Math.Min(delay.TotalSeconds * 2, 60));
                }
            }
            finally
            {
                Interlocked.Exchange(ref _reconnectLoopRunning, 0);
            }
        }

        private async Task DispatchAsync(MqttMessage msg)
        {
            foreach (var kv in _handlers)
            {
                if (TopicMatchesFilter(msg.Topic, kv.Key))
                {
                    foreach (var handler in kv.Value.Values)
                    {
                        _ = Task.Run(() => handler(msg));
                    }
                }
            }

            await Task.CompletedTask;
        }

        private static bool TopicMatchesFilter(string topic, string filter)
        {
            var t = topic.Split('/');
            var f = filter.Split('/');

            int ti = 0;
            for (int fi = 0; fi < f.Length; fi++)
            {
                if (f[fi] == "#") return true;
                if (ti >= t.Length) return false;
                if (f[fi] != "+" && f[fi] != t[ti]) return false;
                ti++;
            }
            return ti == t.Length;
        }

        private static string ParseHost(string brokerUrl)
        {
            if (string.IsNullOrWhiteSpace(brokerUrl))
                return string.Empty;

            if (brokerUrl.Contains("://"))
            {
                var host = brokerUrl[(brokerUrl.IndexOf("://") + 3)..];
                var slash = host.IndexOf('/');
                if (slash >= 0) host = host[..slash];
                return host;
            }

            return brokerUrl;
        }

        // ======================================================
        // CHUNKING
        // ======================================================
        private sealed class ChunkHeader
        {
            public string id { get; set; } = default!;
            public int index { get; set; }
            public int total { get; set; }
            public int size { get; set; }
        }

        private async Task HandleChunkAsync(MqttMessage msg)
        {
            try
            {
                var raw = Encoding.UTF8.GetString(msg.Payload);
                var parts = raw.Split("\n\n", 2);
                if (parts.Length != 2) return;

                var header = JsonSerializer.Deserialize<ChunkHeader>(parts[0]);
                if (header == null) return;

                _chunkTimestamps.TryAdd(header.id, DateTimeOffset.UtcNow);
                var chunkData = Encoding.UTF8.GetBytes(parts[1]);

                var buffer = _chunkBuffer.GetOrAdd(header.id,
                    _ => new SortedDictionary<int, byte[]>());

                lock (buffer)
                {
                    buffer[header.index] = chunkData;
                }

                if (buffer.Count == header.total)
                {
                    var fullPayload = buffer.Values.SelectMany(b => b).ToArray();
                    _chunkBuffer.TryRemove(header.id, out _);
                    _chunkTimestamps.TryRemove(header.id, out _);

                    await DispatchAsync(new MqttMessage
                    {
                        Topic = msg.Topic.Replace("/_chunk", ""),
                        Payload = fullPayload,
                        QoS = msg.QoS,
                        Retain = msg.Retain,
                        ReceivedAt = DateTimeOffset.UtcNow
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Chunk reassembly failed");
            }
        }

        private async Task PublishInternalAsync(
            string topic,
            byte[] payload,
            int qos,
            bool retain,
            CancellationToken ct)
        {
            if (!_client.IsConnected)
                await ConnectAsync(ct);

            var message = new MqttApplicationMessageBuilder()
                .WithTopic(topic)
                .WithPayload(payload)
                .WithQualityOfServiceLevel((MqttQualityOfServiceLevel)qos)
                .WithRetainFlag(retain)
                .Build();

            using var activity = Telemetry.Activity.StartActivity(
                "mqtt.message.publish",
                ActivityKind.Producer);

            try
            {
                await _client.PublishAsync(message, ct);
                Telemetry.MqttPublishes.Add(1);
            }
            catch (Exception ex)
            {
                Telemetry.MqttPublishErrors.Add(1);
                activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                BufferOffline(topic, payload, qos, retain);
            }
        }

        private void BufferOffline(string topic, byte[] payload, int qos, bool retain)
        {
            while (_offlineBuffer.Count >= _offlineMax && _offlineBuffer.TryDequeue(out _)) { }
            _offlineBuffer.Enqueue(new PendingPublish(topic, payload, qos, retain));
        }

        private async Task PublishChunkedAsync(
            string topic,
            byte[] payload,
            int qos,
            bool retain,
            CancellationToken ct)
        {
            var id = Guid.NewGuid().ToString("N");
            var total = (int)Math.Ceiling(payload.Length / (double)_chunkSizeBytes);

            for (int i = 0; i < total; i++)
            {
                var offset = i * _chunkSizeBytes;
                var size = Math.Min(_chunkSizeBytes, payload.Length - offset);

                var chunk = new byte[size];
                Buffer.BlockCopy(payload, offset, chunk, 0, size);

                var header = JsonSerializer.SerializeToUtf8Bytes(new
                {
                    id,
                    index = i,
                    total,
                    size,
                    ts = DateTimeOffset.UtcNow
                });

                var framed = header
                    .Concat(Encoding.UTF8.GetBytes("\n\n"))
                    .Concat(chunk)
                    .ToArray();

                await PublishInternalAsync($"{topic}/_chunk", framed, qos, retain, ct);
            }
        }

        private async Task StartChunkCleanupLoopAsync(CancellationToken token)
        {
            try
            {
                while (!token.IsCancellationRequested)
                {
                    await Task.Delay(TimeSpan.FromSeconds(30), token);

                    var now = DateTimeOffset.UtcNow;
                    foreach (var kv in _chunkTimestamps.ToList())
                    {
                        if (now - kv.Value > _chunkTtl)
                        {
                            _chunkBuffer.TryRemove(kv.Key, out _);
                            _chunkTimestamps.TryRemove(kv.Key, out _);
                        }
                    }
                }
            }
            catch (OperationCanceledException) { }
        }

        private async Task ResubscribeAllAsync()
        {
            try
            {
                var qos = (MqttQualityOfServiceLevel)Math.Clamp(_settings.QoS, 0, 2);
                var filters = _handlers.Keys.ToList();

                foreach (var filter in filters)
                {
                    var attempts = 0;
                    var delay = TimeSpan.FromMilliseconds(300);

                    while (attempts < 3)
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
                            await Task.Delay(delay);
                            delay = TimeSpan.FromMilliseconds(Math.Min(delay.TotalMilliseconds * 2, 5000));
                        }
                        catch
                        {
                            attempts++;
                            await Task.Delay(delay);
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
                    }
                    catch
                    {
                        _offlineBuffer.Enqueue(p);
                        break;
                    }
                }
            }
            finally
            {
                if (taken) _flushLock.Release();
            }
        }

        // ======================================================
        // CLEANUP
        // ======================================================
        public async ValueTask DisposeAsync()
        {
            try
            {
                _chunkCleanupCts.Cancel();

                if (_settingsProvider != null)
                    _settingsProvider.SettingsChanged -= OnSettingsChanged;

                await DisconnectAsync();

                _client.Dispose();
                _connectLock.Dispose();
                _flushLock.Dispose();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error disposing MQTT client");
            }
        }
    }
}
