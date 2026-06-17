using Confluent.Kafka;
using FactoryOperation_NotificationService.FactoryOpsApp.Application.Interfaces.Kafka_Notification;
using FactoryOperation_NotificationService.FactoryOpsApp.Application.Interfaces.Services.EventTrace;
using FactoryOperation_NotificationService.FactoryOpsApp.Application.Interfaces.Services.Shared;
using FactoryOperation_NotificationService.FactoryOpsApp.Application.Models;
using FactoryOperation_NotificationService.FactoryOpsApp.Common.Models;
using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace FactoryOperation_NotificationService.FactoryOpsApp.Infrastructure.Implementation.Kafka
{
    public sealed class KafkaConsumerWorker : BackgroundService
    {
        private readonly ILogger<KafkaConsumerWorker> _logger;
        private readonly KafkaSettings _settings;
        private readonly IServiceScopeFactory _scopeFactory;

        private const string DlqSuffix = "-dlq";

        public KafkaConsumerWorker(
            ILogger<KafkaConsumerWorker> logger,
            IOptions<KafkaSettings> settings,
            IServiceScopeFactory scopeFactory
            )
        {
            _logger = logger;
            _settings = settings.Value;
            _scopeFactory = scopeFactory;
        }

        // ================= SSL CONFIG (EXTENDED ONLY) =================
        private void ApplySecurityConfig(ClientConfig config)
        {
            if (!_settings.EnableSSL)
            {
                config.SecurityProtocol = SecurityProtocol.Plaintext;
                return;
            }

            if (_settings.Security == null)
                return;

            var sec = _settings.Security;

            if (!string.IsNullOrWhiteSpace(sec.Protocol))
                config.SecurityProtocol = Enum.Parse<SecurityProtocol>(sec.Protocol, true);

            if (!string.IsNullOrWhiteSpace(sec.SaslMechanism))
                config.SaslMechanism = Enum.Parse<SaslMechanism>(sec.SaslMechanism, true);

            config.SaslUsername = sec.SaslUsername;
            config.SaslPassword = sec.SaslPassword;

            if (!string.IsNullOrEmpty(sec.SslCAPath))
                config.SslCaLocation = sec.SslCAPath;

            if (!string.IsNullOrEmpty(sec.SslCertificatePath))
                config.SslCertificateLocation = sec.SslCertificatePath;

            if (!string.IsNullOrEmpty(sec.SslKeyPath))
                config.SslKeyLocation = sec.SslKeyPath;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("KafkaConsumerWorker starting…");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await StartConsumer(stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogCritical(ex, "Kafka consumer crashed. Restarting in 10 seconds...");
                    await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
                }
            }

            _logger.LogInformation("KafkaConsumerWorker stopped.");
        }

        private async Task StartConsumer(CancellationToken stoppingToken)
        {
            var regex = new Regex(_settings.TopicPattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);

            var config = new ConsumerConfig
            {
                BootstrapServers = _settings.BootstrapServers,
                GroupId = _settings.GroupId,

                AutoOffsetReset = AutoOffsetReset.Earliest,
                EnableAutoCommit = false,
                EnableAutoOffsetStore = false,

                ClientId = $"factoryops-notification-{Environment.MachineName}",
                SocketKeepaliveEnable = true,

                FetchMinBytes = 1,
                FetchWaitMaxMs = 100,
                MaxPartitionFetchBytes = 1_048_576,
                FetchMaxBytes = 5_242_880,

                MaxPollIntervalMs = 300_000,
                SessionTimeoutMs = 10000,
                HeartbeatIntervalMs = 3000,

                MetadataMaxAgeMs = 5000,

                TopicMetadataRefreshIntervalMs = 5000
            };

            ApplySecurityConfig(config);

            using var consumer = new ConsumerBuilder<string, byte[]>(config)

                .SetErrorHandler((_, e) =>
                {
                    _logger.LogError("Kafka error: {Reason}", e.Reason);
                })

                .SetPartitionsAssignedHandler((c, partitions) =>
                {
                    _logger.LogInformation(
                        "Partitions assigned: [{Partitions}]",
                        string.Join(", ", partitions));
                })

                .SetPartitionsRevokedHandler((c, partitions) =>
                {
                    _logger.LogWarning(
                        "Partitions revoked: [{Partitions}]",
                        string.Join(", ", partitions));
                })

                .Build();

            consumer.Subscribe(_settings.TopicPattern);

            _logger.LogInformation(
            "Kafka consumer subscribed using pattern: {Pattern}",
            _settings.TopicPattern);


            while (!stoppingToken.IsCancellationRequested)
            {
                ConsumeResult<string, byte[]>? cr = null;

                try
                {
                    cr = consumer.Consume(stoppingToken);
                    if (cr == null)
                        continue;

                    _logger.LogInformation(
                          "Message received Topic={Topic} Partition={Partition} Offset={Offset}",
                          cr.Topic,
                          cr.Partition.Value,
                          cr.Offset.Value);

                    var envelope = TryReadEnvelope(cr.Message.Value);

                    using var scope = _scopeFactory.CreateScope();
                    var _trace = scope.ServiceProvider.GetRequiredService<IEventTraceLogger>();

                    await _trace.TrackAsync(new EventTraceEntry
                    {
                        CorrelationId = envelope?.CorrelationId,
                        TenantId = envelope?.TenantId,
                        Service = "NotificationConsumerWorker",
                        Stage = "CONSUME_RECEIVED",
                        Topic = cr.Topic,
                        Partition = cr.Partition.Value,
                        Offset = cr.Offset.Value,
                        Status = "SUCCESS",
                        Message = "Kafka message received"
                    });

                    _logger.LogDebug(
                        "Consumed topic={Topic} p={Partition} o={Offset} size={Size}",
                        cr.Topic,
                        cr.Partition,
                        cr.Offset,
                        cr.Message.Value?.Length ?? 0);

                    var rawBytes = cr.Message.Value ?? Array.Empty<byte>();

                    var processor = scope.ServiceProvider
                        .GetRequiredService<INotificationProcessor>();

                    await _trace.TrackAsync(new EventTraceEntry
                    {
                        CorrelationId = envelope?.CorrelationId,
                        TenantId = envelope?.TenantId,
                        Service = "NotificationConsumerWorker",
                        Stage = "PROCESSING_STARTED",
                        Topic = cr.Topic,
                        Partition = cr.Partition.Value,
                        Offset = cr.Offset.Value,
                        Status = "SUCCESS",
                        Message = "Processing started"
                    });

                    var resolvedPayload = await ResolvePayloadAsync(
                        rawBytes,
                        scope,
                        stoppingToken);

                    await processor.ProcessEventAsync(
                        cr.Topic,
                        resolvedPayload);

                    consumer.StoreOffset(cr);
                    consumer.Commit(cr);

                    await _trace.TrackAsync(new EventTraceEntry
                    {
                        CorrelationId = envelope?.CorrelationId,
                        TenantId = envelope?.TenantId,
                        Service = "NotificationConsumerWorker",
                        Stage = "PROCESSED",
                        Topic = cr.Topic,
                        Partition = cr.Partition.Value,
                        Offset = cr.Offset.Value,
                        Status = "SUCCESS",
                        Message = "Kafka message processed successfully"
                    });

                    _logger.LogInformation(
                        "Kafka message processed: {Topic} Offset={Offset}",
                        cr.Topic,
                        cr.Offset);
                }
                catch (ConsumeException ex)
                {
                    _logger.LogError(ex,
                        "Kafka consume error Topic={Topic} Partition={Partition} Offset={Offset}",
                        ex.ConsumerRecord?.Topic,
                        ex.ConsumerRecord?.Partition,
                        ex.ConsumerRecord?.Offset);

                    if (cr != null)
                    {
                        await TraceFailure(cr, ex);
                        await SendToDlqAsync(consumer, cr, ex, stoppingToken);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Kafka worker unexpected error");

                    if (cr != null)
                    {
                        await TraceFailure(cr, ex);
                        await SendToDlqAsync(consumer, cr, ex, stoppingToken);
                    }
                }
            }

            _logger.LogInformation("Closing Kafka consumer...");
            consumer.Close();
        }

        // ===================== DLQ =====================
        private async Task SendToDlqAsync(
            IConsumer<string, byte[]> consumer,
            ConsumeResult<string, byte[]> cr,
            Exception ex,
            CancellationToken ct)
        {
            try
            {
                var dlqTopic = $"{cr.Topic}{DlqSuffix}";
                var envelope = TryReadEnvelope(cr.Message.Value);

                var producerConfig = new ProducerConfig
                {
                    BootstrapServers = _settings.BootstrapServers
                };

                ApplySecurityConfig(producerConfig);

                using var producer = new ProducerBuilder<string, byte[]>(producerConfig).Build();

                var headers = new Headers
                {
                    { "x-error", Encoding.UTF8.GetBytes(ex.Message) },
                    { "x-original-topic", Encoding.UTF8.GetBytes(cr.Topic) },
                    { "x-original-partition", Encoding.UTF8.GetBytes(cr.Partition.Value.ToString()) },
                    { "x-original-offset", Encoding.UTF8.GetBytes(cr.Offset.Value.ToString()) }
                };

                var res = await producer.ProduceAsync(
                    dlqTopic,
                    new Message<string, byte[]>
                    {
                        Key = cr.Message.Key,
                        Value = cr.Message.Value ?? Array.Empty<byte>(),
                        Headers = headers
                    },
                    ct);

                consumer.StoreOffset(cr);
                consumer.Commit(cr);
                
                using var scope = _scopeFactory.CreateScope();
                var _trace = scope.ServiceProvider.GetRequiredService<IEventTraceLogger>();
                
                await _trace.TrackAsync(new EventTraceEntry
                {
                    CorrelationId = envelope?.CorrelationId,
                    TenantId = envelope?.TenantId,
                    Service = "NotificationConsumerWorker",
                    Stage = "DLQ_ROUTED",
                    Topic = dlqTopic,
                    Partition = res.Partition.Value,
                    Offset = res.Offset.Value,
                    Status = "WARNING",
                    Message = "Message routed to DLQ"
                });

                _logger.LogWarning(
                    "Poison message sent to DLQ {DlqTopic} (offset {Offset})",
                    dlqTopic,
                    cr.Offset.Value);
            }
            catch (Exception e)
            {
                _logger.LogCritical(e, "Failed to route poison message to DLQ");
            }
        }

        // ===================== TOPIC DISCOVERY =====================
        private Task<List<string>> GetMatchingTopicsAsync(
            string bootstrapServers,
            Regex regex,
            ILogger logger)
        {
            var adminConfig = new AdminClientConfig
            {
                BootstrapServers = bootstrapServers
            };

            ApplySecurityConfig(adminConfig);

            using var admin = new AdminClientBuilder(adminConfig).Build();

            var metadata = admin.GetMetadata(TimeSpan.FromSeconds(10));

            var topics = metadata.Topics
                .Where(t => !t.Error.IsError && regex.IsMatch(t.Topic))
                .Select(t => t.Topic)
                .ToList();

            logger.LogInformation("Matched {Count} topics: {Topics}",
                topics.Count,
                string.Join(", ", topics));

            return Task.FromResult(topics);
        }

        private async Task<byte[]> ResolvePayloadAsync(
            byte[] rawBytes,
            IServiceScope scope,
            CancellationToken ct)
        {
            var rawString = Encoding.UTF8.GetString(rawBytes);

            if (!rawString.Contains("\"Type\":\"EXTERNAL_PAYLOAD\""))
                return rawBytes;

            var reference = JsonSerializer.Deserialize<ExternalPayloadKafkaMessage>(rawString);
            if (reference?.Type != "EXTERNAL_PAYLOAD")
                return rawBytes;

            _logger.LogInformation("Downloading external payload from {Url}", reference.BlobUrl);

            var reader = scope.ServiceProvider
                .GetRequiredService<IExternalPayloadReader>();

            return await reader.ReadAsync(reference.BlobUrl, ct);
        }

        private static KafkaMessageEnvelope? TryReadEnvelope(byte[]? bytes)
        {
            if (bytes == null || bytes.Length == 0)
                return null;

            try
            {
                var json = Encoding.UTF8.GetString(bytes);
                return JsonSerializer.Deserialize<KafkaMessageEnvelope>(json);
            }
            catch
            {
                return null;
            }
        }

        private async Task TraceFailure(
            ConsumeResult<string, byte[]> cr,
            Exception ex)
        {
            KafkaMessageEnvelope? envelope = null;

            try
            {
                envelope = TryReadEnvelope(cr.Message.Value);
            }
            catch { }

            using var scope = _scopeFactory.CreateScope();
            var _trace = scope.ServiceProvider.GetRequiredService<IEventTraceLogger>();

            await _trace.TrackAsync(new EventTraceEntry
            {
                CorrelationId = envelope?.CorrelationId,
                TenantId = envelope?.TenantId,
                Service = "NotificationConsumerWorker",
                Stage = "FAILED",
                Topic = cr.Topic,
                Partition = cr.Partition.Value,
                Offset = cr.Offset.Value,
                Status = "ERROR",
                Error = ex.Message
            });
        }
    }
}