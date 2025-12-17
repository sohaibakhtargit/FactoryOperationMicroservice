using Confluent.Kafka;
using FactoryOperation_KafkaMqttService.FactoryOpsApp.Shared.Config;
using FactoryOperation_KafkaMqttService.FactoryOpsApp.Shared.Interfaces;
using FactoryOperation_KafkaMqttService.FactoryOpsApp.Shared.Observability;
using Microsoft.Extensions.Options;
using System.Diagnostics;

namespace FactoryOpsApp.Shared.Services
{
    public class KafkaConsumerService : IKafkaConsumerService, IDisposable
    {
        private readonly KafkaSettings _settings;
        private readonly ILogger<KafkaConsumerService> _logger;
        private readonly IConsumer<string, byte[]> _consumer;
        private CancellationTokenSource? _cts;

        public KafkaConsumerService(IOptions<KafkaSettings> options, ILogger<KafkaConsumerService> logger)
        {
            _settings = options?.Value ?? throw new ArgumentNullException(nameof(options));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            if (string.IsNullOrWhiteSpace(_settings.BootstrapServers))
                throw new ArgumentException("Kafka bootstrap servers cannot be empty", nameof(_settings.BootstrapServers));

            if (string.IsNullOrWhiteSpace(_settings.GroupId))
                throw new ArgumentException("Kafka consumer group ID cannot be empty", nameof(_settings.GroupId));

            var config = new ConsumerConfig
            {
                BootstrapServers = _settings.BootstrapServers,
                GroupId = _settings.GroupId,
                AutoOffsetReset = AutoOffsetReset.Earliest,
                EnableAutoCommit = false
            };

            // Apply optional overrides (including client.id)
            if (_settings.ConsumerConfig != null)
            {
                foreach (var kv in _settings.ConsumerConfig)
                {
                    if (!string.IsNullOrWhiteSpace(kv.Key) && kv.Value is not null)
                        config.Set(kv.Key, kv.Value.ToString());
                }
            }

            // Security
            ApplySecurity(config, _settings);

            _consumer = new ConsumerBuilder<string, byte[]>(config)
                .SetErrorHandler((c, e) =>
                {
                    try
                    {
                        _logger.LogError("Kafka consumer error: {Reason}, IsFatal: {IsFatal}, Code: {Code}",
                            e.Reason, e.IsFatal, e.Code);
                    }
                    catch { /* ignore logger failures */ }
                })
                .SetPartitionsAssignedHandler((c, partitions) =>
                {
                    try
                    {
                        _logger.LogInformation("Partitions assigned: {Partitions}", string.Join(',', partitions));
                    }
                    catch { }
                })
                .SetPartitionsRevokedHandler((c, partitions) =>
                {
                    try
                    {
                        _logger.LogInformation("Partitions revoked: {Partitions}", string.Join(',', partitions));
                    }
                    catch { }
                })
                .Build();

            _logger.LogInformation("✅ Kafka consumer initialized for group '{GroupId}' on '{BootstrapServers}'",
                _settings.GroupId, _settings.BootstrapServers);
        }

        public async Task StartConsumingAsync(string topic, Func<ConsumeResult<string, byte[]>, Task> handler, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(topic))
                throw new ArgumentException("Kafka topic cannot be empty", nameof(topic));

            if (handler == null)
                throw new ArgumentNullException(nameof(handler));

            _cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            var linked = _cts.Token;
            _consumer.Subscribe(topic);

            _logger.LogInformation("📡 Subscribed to Kafka topic: {Topic}", topic);

            try
            {
                while (!linked.IsCancellationRequested)
                {
                    try
                    {
                        var result = _consumer.Consume(linked);
                        if (result is null) continue;

                        using (var activity = Telemetry.Activity.StartActivity("kafka.consume", ActivityKind.Consumer))
                        {
                            activity?.SetTag("messaging.system", "kafka");
                            activity?.SetTag("messaging.destination", result.Topic);
                            activity?.SetTag("messaging.kafka.partition", result.Partition.Value);
                            activity?.SetTag("messaging.kafka.offset", result.Offset.Value);
                            if (!string.IsNullOrEmpty(result.Message.Key))
                                activity?.SetTag("messaging.kafka.message_key", result.Message.Key);

                            await handler(result);
                        }

                        _consumer.Commit(result);
                    }
                    catch (ConsumeException ex)
                    {
                        _logger.LogError(ex, "Kafka consume exception: {Reason}", ex.Error.Reason);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Kafka consumer stopped (cancellation requested).");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in Kafka consumer loop.");
            }
        }

        public async Task StopAsync()
        {
            try
            {
                _cts?.Cancel();
                await Task.Run(() =>
                {
                    _consumer.Close();
                    _logger.LogInformation("Kafka consumer closed successfully.");
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error stopping Kafka consumer");
            }
        }

        public void Dispose()
        {
            try
            {
                _consumer?.Close();
                _consumer?.Dispose();
                _cts?.Dispose();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error disposing Kafka consumer");
            }
        }

        private static void ApplySecurity(ConsumerConfig config, KafkaSettings settings)
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

            // SSL CA location (optional)
            if (!string.IsNullOrEmpty(sec.SslCAPath) && System.IO.File.Exists(sec.SslCAPath))
            {
                config.SslCaLocation = sec.SslCAPath;
            }
        }
    }
}