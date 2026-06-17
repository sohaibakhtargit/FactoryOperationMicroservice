using Confluent.Kafka;
using FactoryOperation_KafkaMqttService.FactoryOpsApp.Application.Interfaces.Services.EventTrace;
using FactoryOperation_KafkaMqttService.FactoryOpsApp.Application.Models;
using FactoryOperation_KafkaMqttService.FactoryOpsApp.Common.CommonConstantFiles;
using FactoryOperation_KafkaMqttService.FactoryOpsApp.Messaging.Interfaces;
using FactoryOperation_KafkaMqttService.FactoryOpsApp.Shared.Config;
using FactoryOperation_KafkaMqttService.FactoryOpsApp.Shared.Interfaces;
using FactoryOperation_KafkaMqttService.FactoryOpsApp.Shared.Models;
using FactoryOps.Shared.Observability;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using static FactoryOperation_KafkaMqttService.FactoryOpsApp.Common.CommonConstantFiles.EventTraceConstants;

namespace FactoryOperation_KafkaMqttService.FactoryOpsApp.Shared.Services
{
    /// Production-grade Kafka producer.
    /// DB-driven, hot-reload safe, backward compatible.
    
    public sealed class KafkaProducerService : IKafkaProducerService, IAsyncDisposable
    {
        private KafkaSettings _settings;
        private readonly ILogger<KafkaProducerService> _logger;
        private readonly IMessagingSettingsProvider _settingsProvider;

        private readonly object _sync = new();
        private IProducer<string, byte[]> _producer = default!;

        private string _dlqTopic = default!;
        private int _dlqMaxRetries;
        private int _dlqBackoffMs;

        private const int MaxProduceRetries = 3;
        private readonly IServiceScopeFactory _scopeFactory;

        public KafkaProducerService(
        IOptions<KafkaSettings> options,
        ILogger<KafkaProducerService> logger,
        IMessagingSettingsProvider settingsProvider,
        IServiceScopeFactory scopeFactory)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _settingsProvider = settingsProvider ?? throw new ArgumentNullException(nameof(settingsProvider));

            _settings = options.Value ?? throw new ArgumentNullException(nameof(options));
            _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
            // Load DB config (global default)
            ReloadSettings(null);

            _settingsProvider.SettingsChanged += (_, tenantId) =>
            {
                try
                {
                    ReloadSettings(tenantId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Kafka producer config reload failed");
                }
            };
        }

        private void OnSettingsChanged(object? sender, int? tenantId)
        {
           
            if (tenantId.HasValue)
                return;

            try
            {
                _logger.LogInformation("Reloading Kafka producer settings from DB");
                ReloadSettings(null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to reload Kafka producer settings");
            }
        }

        private void ReloadSettings(int? tenantId)
        {
            var newSettings = _settingsProvider
                .GetEffectiveSettings(tenantId)
                .Kafka;

            var newProducer = BuildProducer(newSettings);

            lock (_sync)
            {
                var old = _producer;
                _producer = newProducer;
                _settings = newSettings;

                _dlqTopic = _settings.Dlq?.Topic ?? $"{_settings.Topic}-dlq";
                _dlqMaxRetries = _settings.Dlq?.MaxRetries ?? 5;
                _dlqBackoffMs = _settings.Dlq?.RetryBackoffMs ?? 500;

                try
                {
                    old?.Flush(TimeSpan.FromSeconds(5));
                    old?.Dispose();
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Old Kafka producer shutdown warning");
                }
            }

            _logger.LogInformation(
                "Kafka producer reloaded (Bootstrap={Bootstrap}, ClientId={ClientId})",
                newSettings.BootstrapServers,
                newSettings.ProducerConfig?.TryGetValue("client.id", out var cid) == true
                    ? cid
                    : "factoryops-producer");
        }

        private IProducer<string, byte[]> BuildProducer(KafkaSettings settings)
        {
            var config = new ProducerConfig
            {
                BootstrapServers = settings.BootstrapServers,
                Acks = KafkaClientDefaults.ProducerAcks,
                EnableIdempotence = KafkaClientDefaults.EnableIdempotence,
                LingerMs = KafkaClientDefaults.LingerMs,
                BatchSize = KafkaClientDefaults.BatchSize,
                CompressionType = KafkaClientDefaults.Compression,
                MessageTimeoutMs = KafkaClientDefaults.MessageTimeoutMs,
                ClientId = (settings.ProducerConfig != null &&
                            settings.ProducerConfig.TryGetValue("client.id", out var cid) &&
                            cid is not null)
                    ? cid.ToString()!
                    : $"factoryops-producer-{Environment.MachineName}"
            };

            ApplySecurity(config, settings);

            if (settings.ProducerConfig != null)
            {
                foreach (var kv in settings.ProducerConfig)
                {
                    if (!string.IsNullOrWhiteSpace(kv.Key) && kv.Value is not null)
                        config.Set(kv.Key, kv.Value.ToString());
                }
            }

            TrySetDefault(config, settings.ProducerConfig, "retries",
                KafkaClientDefaults.Retries.ToString());

            TrySetDefault(config, settings.ProducerConfig, "retry.backoff.ms",
                KafkaClientDefaults.RetryBackoffMs.ToString());

            TrySetDefault(config, settings.ProducerConfig, "max.in.flight.requests.per.connection",
                KafkaClientDefaults.MaxInFlightRequests.ToString());

            TrySetDefault(config, settings.ProducerConfig, "socket.keepalive.enable",
                KafkaClientDefaults.SocketKeepAlive.ToString().ToLower());

            TrySetDefault(config, settings.ProducerConfig, "queue.buffering.max.kbytes",
                KafkaClientDefaults.QueueBufferingMaxKb.ToString());

            TrySetDefault(config, settings.ProducerConfig, "message.max.bytes",
                KafkaClientDefaults.MessageMaxBytes.ToString());

            return new ProducerBuilder<string, byte[]>(config)
                .SetErrorHandler((p, e) =>
                {
                    _logger.LogError(LogMessages.KafkaProducerError,
                     e.Reason, e.IsFatal, e.Code);
                })
                .SetLogHandler((p, m) =>
                {
                    _logger.LogDebug("Kafka producer: {Message}", m.Message);
                })
                .Build();
        }
        public async Task ProduceAsync(
            string topic,
            string key,
            byte[] payload,
            CancellationToken ct = default)
        {

            var msg = new Message<string, byte[]>
            {
                Key = key,
                Value = payload
            };

            IProducer<string, byte[]> producer;
            lock (_sync)
                producer = _producer;

            await producer
                .ProduceAsync(topic, msg, ct)
                .ConfigureAwait(false);
        }


        public async Task ProduceEnvelopeAsync(
            string topic,
            KafkaMessageEnvelope envelope,
            CancellationToken ct = default)
        {
            using var scope = _scopeFactory.CreateScope();
            var _trace = scope.ServiceProvider.GetRequiredService<IEventTraceLogger>();

            await _trace.TrackAsync(new EventTraceEntry
            {
                CorrelationId = envelope.CorrelationId,
                TenantId = envelope.TenantId,
                Service = EventTraceConstants.Services.KafkaProducer,
                Stage = Stages.ProduceRequested,
                Topic = topic,
                Status = Status.Success,
                Message = Messages.KafkaProduceRequested
            });

            var payload = JsonSerializer.SerializeToUtf8Bytes(envelope);

            await ProduceInternalAsync(
                topic,
                envelope.Key,
                payload,
                envelope,
                ct).ConfigureAwait(false);
        }


        private async Task ProduceInternalAsync(
            string topic,
            string key,
            byte[] payload,
            KafkaMessageEnvelope envelope,
            CancellationToken ct)
        {
            using var activity = Telemetry.Activity.StartActivity("kafka.produce", ActivityKind.Producer);
            activity?.SetTag("messaging.system", "kafka");
            activity?.SetTag("messaging.destination", topic);
            activity?.SetTag("messaging.kafka.message_key", key);

            var sw = Stopwatch.StartNew();
            var attempt = 0;

            while (true)
            {
                IProducer<string, byte[]> producer;
                lock (_sync)
                    producer = _producer;

                try
                {
                    var msg = new Message<string, byte[]>
                    {
                        Key = key,
                        Value = payload
                    };

                    var delivery = await producer
                        .ProduceAsync(topic, msg, ct)
                        .ConfigureAwait(false);

                    sw.Stop();

                    activity?.SetTag("messaging.kafka.partition", delivery.Partition.Value);
                    activity?.SetTag("messaging.kafka.offset", delivery.Offset.Value);

                    try
                    {
                        Telemetry.KafkaMessagesProduced.Add(
                            1,
                            new KeyValuePair<string, object?>("topic", topic));

                        Telemetry.KafkaProduceLatencyMs.Record(
                            sw.Elapsed.TotalMilliseconds,
                            new KeyValuePair<string, object?>("topic", topic));
                    }
                    catch { }

                    _logger.LogInformation(
                         LogMessages.KafkaProduced,
                         delivery.Topic,
                         key,
                         delivery.Partition.Value,
                         delivery.Offset.Value);

                    using var scope = _scopeFactory.CreateScope();
                    var _trace = scope.ServiceProvider.GetRequiredService<IEventTraceLogger>();

                    await _trace.TrackAsync(new EventTraceEntry
                    {
                        CorrelationId = envelope.CorrelationId,
                        TenantId = envelope.TenantId,
                        Service = EventTraceConstants.Services.KafkaProducer,
                        Stage = Stages.Produced,
                        Topic = topic,
                        Partition = delivery.Partition.Value,
                        Offset = delivery.Offset.Value,
                        Status = Status.Success,
                        Message = EventTraceConstants.Messages.KafkaProduced
                    });

                    return;
                }
                catch (KafkaException ex) when (ex.Error.Code == ErrorCode.Local_QueueFull)
                {
                    sw.Stop();
                    attempt++;

                    _logger.LogWarning(
                        "Kafka buffer full (attempt {Attempt}/{Max}). Backing off {Backoff}ms",
                        attempt,
                        MaxProduceRetries,
                        _dlqBackoffMs);

                    if (attempt >= MaxProduceRetries)
                    {
                        await TraceFailure(topic, envelope, ex.Message);
                        throw;
                    }

                    await Task.Delay(_dlqBackoffMs, ct).ConfigureAwait(false);
                }
                catch (ProduceException<string, byte[]> ex)
                {
                    sw.Stop();
                    attempt++;

                    try
                    {
                        Telemetry.KafkaProduceErrors.Add(
                            1,
                            new KeyValuePair<string, object?>("topic", topic));
                    }
                    catch { }

                    activity?.SetStatus(ActivityStatusCode.Error, ex.Error.Reason);

                    _logger.LogError(
                        ex,
                        "Produce failed for topic {Topic} key {Key} (attempt {Attempt}/{Max})",
                        topic,
                        key,
                        attempt,
                        MaxProduceRetries);

                    if (attempt >= MaxProduceRetries)
                    {
                        await TraceFailure(topic, envelope, ex.Error.Reason);
                        await SendToDlqAsync(topic, key, payload, envelope, ex, ct).ConfigureAwait(false);
                        throw;
                    }

                    await Task.Delay(_dlqBackoffMs, ct).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    sw.Stop();
                    attempt++;

                    try
                    {
                        Telemetry.KafkaProduceErrors.Add(
                            1,
                            new KeyValuePair<string, object?>("topic", topic));
                    }
                    catch { }

                    activity?.SetStatus(ActivityStatusCode.Error, ex.Message);

                    _logger.LogError(
                        ex,
                        "Unexpected error during Produce for topic {Topic} key {Key} (attempt {Attempt}/{Max})",
                        topic,
                        key,
                        attempt,
                        MaxProduceRetries);

                    if (attempt >= MaxProduceRetries)
                    {
                        await TraceFailure(topic, envelope, ex.Message);
                        await SendToDlqAsync(topic, key, payload, envelope, ex, ct).ConfigureAwait(false);
                        throw;
                    }

                    await Task.Delay(_dlqBackoffMs, ct).ConfigureAwait(false);
                }
            }
        }

        // ======================================================
        // DLQ (UNCHANGED)
        // ======================================================
        private async Task SendToDlqAsync(
            string originalTopic,
            string key,
            byte[] payload,
            KafkaMessageEnvelope envelope,
            Exception ex,
            CancellationToken ct)
        {
            if (!(_settings.Dlq?.Enabled ?? true))
                return;

            var attempt = 0;
            var headers = new Headers();

            if (_settings.Dlq?.AddHeaders ?? true)
            {
                headers.Add("x-error", Encoding.UTF8.GetBytes(ex.Message ?? "unknown"));
                headers.Add("x-original-topic", Encoding.UTF8.GetBytes(originalTopic));
            }

            while (attempt < _dlqMaxRetries)
            {
                try
                {
                    IProducer<string, byte[]> producer;
                    lock (_sync)
                        producer = _producer;

                    var msg = new Message<string, byte[]>
                    {
                        Key = key,
                        Value = payload,
                        Headers = headers
                    };

                    var res = await producer
                        .ProduceAsync(_dlqTopic, msg, ct)
                        .ConfigureAwait(false);

                    _logger.LogWarning(
                        "Routed message to DLQ {DlqTopic} offset {Offset}",
                        _dlqTopic,
                        res.Offset.Value);

                    using var scope = _scopeFactory.CreateScope();
                    var _trace = scope.ServiceProvider.GetRequiredService<IEventTraceLogger>();

                    await _trace.TrackAsync(new EventTraceEntry
                    {
                        CorrelationId = envelope.CorrelationId,
                        TenantId = envelope.TenantId,
                        Service = EventTraceConstants.Services.KafkaProducer,
                        Stage = Stages.DlqRouted,
                        Topic = _dlqTopic,
                        Partition = res.Partition.Value,
                        Offset = res.Offset.Value,
                        Status = Status.Warning,
                        Message = Messages.KafkaDlqRouted
                    });

                    return;
                }
                catch (Exception dex)
                {
                    attempt++;

                    _logger.LogWarning(
                        dex,
                        "Failed to write to DLQ on attempt {Attempt}/{Max}",
                        attempt,
                        _dlqMaxRetries);

                    await Task.Delay(_dlqBackoffMs, ct).ConfigureAwait(false);
                }
            }

            _logger.LogError(
                "Could not route message to DLQ after {Attempts} attempts",
                _dlqMaxRetries);
        }

        // ======================================================
        // HELPERS
        // ======================================================
        private async Task TraceFailure(
            string topic,
            KafkaMessageEnvelope envelope,
            string error)
        {
            using var scope = _scopeFactory.CreateScope();
            var _trace = scope.ServiceProvider.GetRequiredService<IEventTraceLogger>();

            await _trace.TrackAsync(new EventTraceEntry
            {
                CorrelationId = envelope.CorrelationId,
                TenantId = envelope.TenantId,
                Service = EventTraceConstants.Services.KafkaProducer,
                Stage = Stages.Failed,
                Topic = topic,
                Status = Status.Error,
                Error = error
            });
        }

        private static void ApplySecurity(
            ProducerConfig config,
            KafkaSettings settings)
        {
            if (!settings.EnableSSL)
            {
                config.SecurityProtocol = SecurityProtocol.Plaintext;
                return;
            }

            var sec = settings.Security;
            if (sec == null) return;

            if (!string.IsNullOrWhiteSpace(sec.Protocol))
            {
                config.SecurityProtocol = sec.Protocol.ToUpperInvariant() switch
                {
                    "SSL" => SecurityProtocol.Ssl,
                    "SASL_SSL" => SecurityProtocol.SaslSsl,
                    "SASL_PLAINTEXT" => SecurityProtocol.SaslPlaintext,
                    "PLAINTEXT" => SecurityProtocol.Plaintext,
                    _ => config.SecurityProtocol
                };
            }

            if (!string.IsNullOrWhiteSpace(sec.SaslMechanism))
            {
                config.SaslMechanism = sec.SaslMechanism.ToUpperInvariant() switch
                {
                    "PLAIN" => SaslMechanism.Plain,
                    "SCRAM-SHA-256" => SaslMechanism.ScramSha256,
                    "SCRAM-SHA-512" => SaslMechanism.ScramSha512,
                    "OAUTHBEARER" => SaslMechanism.OAuthBearer,
                    _ => config.SaslMechanism
                };
            }

            if (!string.IsNullOrEmpty(sec.SaslUsername))
            {
                config.SaslUsername = sec.SaslUsername;
                config.SaslPassword = sec.SaslPassword;
            }

            if (!string.IsNullOrEmpty(sec.SslCAPath) && File.Exists(sec.SslCAPath))
            {
                config.SslCaLocation = sec.SslCAPath;
            }

            if (!string.IsNullOrEmpty(sec.SslCertificatePath) &&
                File.Exists(sec.SslCertificatePath))
            {
                config.SslCertificateLocation = sec.SslCertificatePath;
            }

            if (!string.IsNullOrEmpty(sec.SslKeyPath) &&
                File.Exists(sec.SslKeyPath))
            {
                config.SslKeyLocation = sec.SslKeyPath;
            }
        }

        private static void TrySetDefault(
            ProducerConfig config,
            IDictionary<string, object>? userConfig,
            string key,
            string value)
        {
            if (userConfig != null &&
                userConfig.Keys.Any(k =>
                    string.Equals(k, key, StringComparison.OrdinalIgnoreCase)))
            {
                return;
            }

            config.Set(key, value);
        }

        // ======================================================
        // SHUTDOWN
        // ======================================================
        public async ValueTask DisposeAsync()
        {
            _settingsProvider.SettingsChanged -= OnSettingsChanged;

            IProducer<string, byte[]> producer;
            lock (_sync)
                producer = _producer;

            try
            {
                producer?.Flush(TimeSpan.FromSeconds(10));
            }
            finally
            {
                producer?.Dispose();
            }

            await Task.CompletedTask;
        }
    }
}
