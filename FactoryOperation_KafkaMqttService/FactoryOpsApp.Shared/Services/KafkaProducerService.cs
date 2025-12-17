using Confluent.Kafka;
using FactoryOperation_KafkaMqttService.FactoryOpsApp.Shared.Config;
using FactoryOperation_KafkaMqttService.FactoryOpsApp.Shared.Interfaces;
using FactoryOperation_KafkaMqttService.FactoryOpsApp.Shared.Models;
using FactoryOperation_KafkaMqttService.FactoryOpsApp.Shared.Observability;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Security.Cryptography.X509Certificates;
using System.IO;

namespace FactoryOperation_KafkaMqttService.FactoryOpsApp.Shared.Services
{

    public class KafkaProducerService : IKafkaProducerService
    {
        private readonly KafkaSettings _settings;
        private readonly ILogger<KafkaProducerService> _logger;
        private readonly IProducer<string, byte[]> _producer;
        private readonly string _dlqTopic;
        private readonly int _dlqMaxRetries;
        private readonly int _dlqBackoffMs;

        public KafkaProducerService(IOptions<KafkaSettings> settings, ILogger<KafkaProducerService> logger)
        {
            _settings = settings.Value ?? throw new ArgumentNullException(nameof(settings));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            var config = new ProducerConfig
            {
                BootstrapServers = _settings.BootstrapServers,
                Acks = Acks.All,
                EnableIdempotence = true,
                // sensible defaults; allow override via ProducerConfig
                MessageTimeoutMs = 120000,
                LingerMs = 20,
                BatchSize = 65536,
                CompressionType = CompressionType.Lz4,
                ClientId = (_settings.ProducerConfig != null && _settings.ProducerConfig.TryGetValue("client.id", out var cid) && cid is not null)
                           ? cid.ToString()!
                           : $"factoryops-producer-{Guid.NewGuid():N}"
            };

            // Security mapping
            ApplySecurity(config, _settings);

            // Apply advanced producer config overrides (user overrides win)
            if (_settings.ProducerConfig != null)
            {
                foreach (var kv in _settings.ProducerConfig)
                {
                    if (!string.IsNullOrWhiteSpace(kv.Key) && kv.Value is not null)
                        config.Set(kv.Key, kv.Value.ToString());
                }
            }

            // Reliability/perf defaults if caller hasn't provided them
            TrySetDefault(config, _settings.ProducerConfig, "retries", int.MaxValue.ToString());
            TrySetDefault(config, _settings.ProducerConfig, "retry.backoff.ms", "100");
            TrySetDefault(config, _settings.ProducerConfig, "max.in.flight.requests.per.connection", "5"); // safe with idempotence
            TrySetDefault(config, _settings.ProducerConfig, "socket.keepalive.enable", "true");
            // delivery.timeout.ms already seeded via MessageTimeoutMs

            _producer = new ProducerBuilder<string, byte[]>(config)
               .SetErrorHandler((p, e) =>
               {
                   _logger.LogError("Kafka producer error: {Reason}, IsFatal: {IsFatal}, Code: {Code}", e.Reason, e.IsFatal, e.Code);
               })
               .SetLogHandler((p, m) => _logger.LogDebug("Kafka producer: {Msg}", m))
               .Build();

            _dlqTopic = _settings.Dlq?.Topic ?? $"{_settings.Topic}-dlq";
            _dlqMaxRetries = _settings.Dlq?.MaxRetries ?? 5;
            _dlqBackoffMs = _settings.Dlq?.RetryBackoffMs ?? 500;

            _logger.LogInformation("✅ Kafka producer initialized (Bootstrap={Bootstrap}, ClientId={ClientId})",
                config.BootstrapServers, config.ClientId);
        }

        public async Task ProduceAsync(string topic, string key, byte[] payload, CancellationToken ct = default)
        {
            using var activity = Telemetry.Activity.StartActivity("kafka.produce", ActivityKind.Producer);
            activity?.SetTag("messaging.system", "kafka");
            activity?.SetTag("messaging.destination", topic);
            activity?.SetTag("messaging.kafka.message_key", key);

            var sw = Stopwatch.StartNew();
            try
            {
                var msg = new Message<string, byte[]> { Key = key, Value = payload };
                var delivery = await _producer.ProduceAsync(topic, msg, ct).ConfigureAwait(false);
                sw.Stop();

                activity?.SetTag("messaging.kafka.partition", delivery.Partition.Value);
                activity?.SetTag("messaging.kafka.offset", delivery.Offset.Value);

                try
                {
                    Telemetry.KafkaMessagesProduced.Add(1, new KeyValuePair<string, object?>("topic", topic));
                    Telemetry.KafkaProduceLatencyMs.Record(sw.Elapsed.TotalMilliseconds, new KeyValuePair<string, object?>("topic", topic));
                }
                catch { }

                _logger.LogInformation("Produced to {Topic} key={Key} p={Partition} o={Offset}", delivery.Topic, key, delivery.Partition.Value, delivery.Offset.Value);
            }
            catch (ProduceException<string, byte[]> ex)
            {
                sw.Stop();
                try { Telemetry.KafkaProduceErrors.Add(1, new KeyValuePair<string, object?>("topic", topic)); } catch { }
                activity?.SetStatus(ActivityStatusCode.Error, ex.Error.Reason);
                _logger.LogError(ex, "Produce failed for topic {Topic} key {Key}", topic, key);
                await SendToDlqAsync(topic, key, payload, ex, ct).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                sw.Stop();
                try { Telemetry.KafkaProduceErrors.Add(1, new KeyValuePair<string, object?>("topic", topic)); } catch { }
                activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                _logger.LogError(ex, "Unexpected error during Produce for topic {Topic} key {Key}", topic, key);
                await SendToDlqAsync(topic, key, payload, ex, ct).ConfigureAwait(false);
            }
        }

        public async Task ProduceEnvelopeAsync(string topic, KafkaMessageEnvelope envelope, CancellationToken ct = default)
        {
            var payload = JsonSerializer.SerializeToUtf8Bytes(envelope);
            await ProduceAsync(topic, envelope.Key, payload, ct).ConfigureAwait(false);
        }

        private async Task SendToDlqAsync(string originalTopic, string key, byte[] payload, Exception ex, CancellationToken ct)
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
                    var msg = new Message<string, byte[]>
                    {
                        Key = key,
                        Value = payload,
                        Headers = headers
                    };
                    var res = await _producer.ProduceAsync(_dlqTopic, msg, ct).ConfigureAwait(false);
                    _logger.LogWarning("Routed message to DLQ {DlqTopic} offset {Offset}", _dlqTopic, res.Offset.Value);
                    return;
                }
                catch (Exception dex)
                {
                    attempt++;
                    _logger.LogWarning(dex, "Failed to write to DLQ on attempt {Attempt}", attempt);
                    await Task.Delay(_dlqBackoffMs, ct).ConfigureAwait(false);
                }
            }

            _logger.LogError("Could not route message to DLQ after {Attempts} attempts", _dlqMaxRetries);
        }

        public void Dispose()
        {
            try { _producer.Flush(TimeSpan.FromSeconds(10)); } catch { /* ignore */ }
            _producer.Dispose();
        }

        private static void ApplySecurity(ProducerConfig config, KafkaSettings settings)
        {
            // Allow disabling SSL entirely via config
            if (!settings.EnableSSL)
            {
                config.SecurityProtocol = SecurityProtocol.Plaintext;
                return;
            }

            var sec = settings.Security;
            if (sec == null) return;

            // Protocol
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

            // SASL
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

            // SSL truststore/CA
            if (!string.IsNullOrEmpty(sec.SslCAPath) && File.Exists(sec.SslCAPath))
            {
                config.SslCaLocation = sec.SslCAPath;
            }

            // mTLS: client cert/key (PEM) if provided
            if (!string.IsNullOrEmpty(sec.SslCertificatePath) && File.Exists(sec.SslCertificatePath))
            {
                config.SslCertificateLocation = sec.SslCertificatePath;
            }
            if (!string.IsNullOrEmpty(sec.SslKeyPath) && File.Exists(sec.SslKeyPath))
            {
                config.SslKeyLocation = sec.SslKeyPath;
            }
        }

        private static void TrySetDefault(ProducerConfig config, IDictionary<string, object>? userConfig, string key, string value)
        {
            if (userConfig != null && userConfig.Keys.Any(k => string.Equals(k, key, StringComparison.OrdinalIgnoreCase)))
                return;
            config.Set(key, value);
        }
    }
}

