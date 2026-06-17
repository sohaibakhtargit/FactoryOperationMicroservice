using Confluent.Kafka;
using FactoryOperation_KafkaMqttService.FactoryOpsApp.Messaging.Interfaces;
using FactoryOperation_KafkaMqttService.FactoryOpsApp.Shared.Config;
using FactoryOperation_KafkaMqttService.FactoryOpsApp.Shared.Interfaces;
using FactoryOperation_KafkaMqttService.FactoryOpsApp.Shared.Models;
using FactoryOps.Shared.Observability;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.Text.Json;

namespace FactoryOpsApp.Shared.Services
{
    /// <summary>
    /// Single Kafka consumer engine.
    /// Used by:
    /// - Application services (IKafkaConsumerService)
    /// - MQTT ↔ Kafka bridge (IKafkaConsumerBridgeService)
    ///
    /// Guarantees:
    /// - ONE Kafka consumer instance
    /// - Tenant-aware, DB-driven topic
    /// - Safe commits
    /// - Full telemetry
    /// </summary>
    public sealed class KafkaConsumerService :
        IKafkaConsumerService,
        IKafkaConsumerBridgeService,
        IDisposable
    {
        private KafkaSettings _settings;
        private readonly ILogger<KafkaConsumerService> _logger;
        private readonly IConsumer<string, byte[]> _consumer;
        private readonly IMessagingSettingsProvider _settingsProvider;

        private CancellationTokenSource? _cts;
        private int _loopRunning = 0;

        public KafkaConsumerService(
            IOptions<KafkaSettings> options,
            ILogger<KafkaConsumerService> logger,
            IMessagingSettingsProvider settingsProvider)
        {
            _settingsProvider = settingsProvider;
            _settings = options?.Value ?? throw new ArgumentNullException(nameof(options));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _settings = _settingsProvider
                .GetEffectiveSettings(null)
                .Kafka;

            if (string.IsNullOrWhiteSpace(_settings.BootstrapServers))
                throw new ArgumentException("Kafka bootstrap servers cannot be empty", nameof(_settings.BootstrapServers));

            if (string.IsNullOrWhiteSpace(_settings.GroupId))
                throw new ArgumentException("Kafka consumer group ID cannot be empty", nameof(_settings.GroupId));

            var config = new ConsumerConfig
            {
                BootstrapServers = _settings.BootstrapServers,
                GroupId = _settings.GroupId,
                AutoOffsetReset = AutoOffsetReset.Earliest,
                EnableAutoCommit = false,
                ClientId = _settings.ConsumerConfig != null &&
                           _settings.ConsumerConfig.TryGetValue("client.id", out var cid)
                    ? cid?.ToString()
                    : $"factoryops-consumer-{Environment.MachineName}"
            };

            if (_settings.ConsumerConfig != null)
            {
                foreach (var kv in _settings.ConsumerConfig)
                {
                    if (!string.IsNullOrWhiteSpace(kv.Key) && kv.Value is not null)
                        config.Set(kv.Key, kv.Value.ToString());
                }
            }

            ApplySecurity(config, _settings);

            _consumer = new ConsumerBuilder<string, byte[]>(config)
                .SetErrorHandler((c, e) =>
                {
                    try
                    {
                        _logger.LogError(
                            "Kafka consumer error: {Reason}, IsFatal: {IsFatal}, Code: {Code}",
                            e.Reason, e.IsFatal, e.Code);
                    }
                    catch {}
                })
                .SetPartitionsAssignedHandler((c, partitions) =>
                {
                    try
                    {
                        _logger.LogInformation(
                            "Partitions assigned: {Partitions}",
                            string.Join(',', partitions));
                    }
                    catch {}
                })
                .SetPartitionsRevokedHandler((c, partitions) =>
                {
                    try
                    {
                        _logger.LogInformation(
                            "Partitions revoked: {Partitions}",
                            string.Join(',', partitions));
                    }
                    catch { }
                })
                .Build();

            _logger.LogInformation(
                "Kafka consumer initialized (Group={GroupId}, Bootstrap={Bootstrap})",
                _settings.GroupId,
                _settings.BootstrapServers);
        }

        // ======================================================
        // BRIDGE ENTRY POINT (REQUIRED)
        // ======================================================
        public async Task ConsumeAsync(
            Func<KafkaMessageEnvelope, Task> handler,
            CancellationToken ct)
        {
            if (handler == null)
                throw new ArgumentNullException(nameof(handler));

            if (Interlocked.Exchange(ref _loopRunning, 1) == 1)
            {
                _logger.LogWarning("Kafka consumer loop already running — ignoring duplicate start");
                return;
            }

            var settings = _settingsProvider.GetEffectiveSettings(null).Kafka;
            var topic = settings.KafkaToMqttTopic ?? settings.ConsumerTopic ?? settings.Topic;

            _logger.LogInformation("Bridge consuming Kafka topic: {Topic}", topic);

            await StartConsumingInternalAsync(
                topic,
                async result =>
                {
                    try
                    {
                        var envelope = JsonSerializer.Deserialize<KafkaMessageEnvelope>(
                            result.Message.Value);

                        if (envelope == null)
                        {
                            _logger.LogWarning("Invalid Kafka message (null envelope)");
                            return;
                        }

                        await handler(envelope);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Bridge handler failed for Kafka message");
                        throw;
                    }
                },
                ct);
        }

        // ======================================================
        // LEGACY API (UNCHANGED)
        // ======================================================
        public async Task StartConsumingAsync(
            string topic,
            Func<ConsumeResult<string, byte[]>, Task> handler,
            CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(topic))
                throw new ArgumentException("Kafka topic cannot be empty", nameof(topic));

            if (handler == null)
                throw new ArgumentNullException(nameof(handler));

            await StartConsumingInternalAsync(topic, handler, ct);
        }

        private async Task StartConsumingInternalAsync(
            string topic,
            Func<ConsumeResult<string, byte[]>, Task> handler,
            CancellationToken ct)
        {
            _cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            var linked = _cts.Token;

            //_consumer.Subscribe(topic);

            if (topic.Contains(@"\d+") || topic.Contains(".+"))
            {
                using var admin = new AdminClientBuilder(
                    new AdminClientConfig
                    {
                        BootstrapServers = _settings.BootstrapServers
                    }).Build();

                var metadata = admin.GetMetadata(TimeSpan.FromSeconds(10));

                var regex = new System.Text.RegularExpressions.Regex(
                    topic,
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase);

                var matchedTopics = metadata.Topics
                    .Where(t => !t.Error.IsError)
                    .Select(t => t.Topic)
                    .Where(t => regex.IsMatch(t))
                    .Distinct()
                    .ToList();

                _logger.LogInformation(
                        "Kafka metadata topics found: {Topics}",
                        string.Join(", ", metadata.Topics.Select(x => x.Topic)));

                if (!matchedTopics.Any())
                {
                    _logger.LogWarning("No Kafka topics matched regex: {Regex}", topic);
                }
                else
                {
                    _consumer.Subscribe(matchedTopics);

                    _logger.LogInformation(
                        "Subscribed to regex topics: {Topics}",
                        string.Join(", ", matchedTopics));
                }
            }
            else
            {
                _consumer.Subscribe(topic);

                _logger.LogInformation("Subscribed to Kafka topic: {Topic}", topic);
            }

            _logger.LogInformation("Subscribed to Kafka topic: {Topic}", topic);

            try
            {
                while (!linked.IsCancellationRequested)
                {
                    try
                    {
                        var result = _consumer.Consume(linked);
                        if (result is null) continue;

                        using var activity = Telemetry.Activity.StartActivity(
                            "kafka.consume",
                            ActivityKind.Consumer);

                        activity?.SetTag("messaging.system", "kafka");
                        activity?.SetTag("messaging.destination", result.Topic);
                        activity?.SetTag("messaging.kafka.partition", result.Partition.Value);
                        activity?.SetTag("messaging.kafka.offset", result.Offset.Value);

                        if (!string.IsNullOrEmpty(result.Message.Key))
                            activity?.SetTag("messaging.kafka.message_key", result.Message.Key);

                        await handler(result);

                        _consumer.Commit(result);
                    }
                    catch (ConsumeException ex)
                    {
                        _logger.LogError(
                            ex,
                            "Kafka consume exception: {Reason}",
                            ex.Error.Reason);
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
            finally
            {
                Interlocked.Exchange(ref _loopRunning, 0);
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

        // ======================================================
        // SECURITY
        // ======================================================
        private static void ApplySecurity(
            ConsumerConfig config,
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

            if (!string.IsNullOrEmpty(sec.SslCAPath) &&
                File.Exists(sec.SslCAPath))
            {
                config.SslCaLocation = sec.SslCAPath;
            }
        }
    }
}
