using Confluent.Kafka;
using FactoryOperation_NotificationService.FactoryOpsApp.Application.Interfaces.Kafka_Notification;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.RegularExpressions;

namespace FactoryOperation_NotificationService.FactoryOpsApp.Infrastructure.Implementation.Kafka
{
    public sealed class KafkaSettings
    {
        public string BootstrapServers { get; set; } = "192.168.0.86:29092";
        public string GroupId { get; set; } = "factoryops-notification.v1"; // use a fresh group id in dev
        public string TopicPattern { get; set; } = "^factoryops\\..+\\..+\\..+$";
        public bool EnableAutoOffsetStore { get; set; } = true;
        public bool EnableAutoCommit { get; set; } = true;
    }

    public sealed class KafkaConsumerWorker : BackgroundService
    {
        private readonly ILogger<KafkaConsumerWorker> _logger;
        private readonly KafkaSettings _settings;
        private readonly IServiceScopeFactory _scopeFactory;

        public KafkaConsumerWorker(
            ILogger<KafkaConsumerWorker> logger,
            IOptions<KafkaSettings> settings,
            IServiceScopeFactory scopeFactory)
        {
            _logger = logger;
            _settings = settings.Value;
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("KafkaConsumerWorker starting…");
            await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
            await StartConsumer(stoppingToken);
        }

        private async Task StartConsumer(CancellationToken stoppingToken)
        {
            var pattern = _settings.TopicPattern;
            var regex = new Regex(pattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);

            var config = new ConsumerConfig
            {
                BootstrapServers = _settings.BootstrapServers,
                GroupId = _settings.GroupId,
                // In dev, use Earliest so you can read existing messages
                AutoOffsetReset = AutoOffsetReset.Earliest,
                EnableAutoCommit = false,
                EnableAutoOffsetStore = false,
                MetadataMaxAgeMs = 30000,
                SocketKeepaliveEnable = true,
                //ExcludeInternalTopics = true,
                ClientId = $"factoryops-notification-{Environment.MachineName}",
                Debug = "cgrp,topic,fetch" // librdkafka debug categories
            };

            using var consumer = new ConsumerBuilder<string, string>(config)
                .SetErrorHandler((_, e) => _logger.LogError("Kafka error: {Reason}", e.Reason))
                .SetLogHandler((_, m) => _logger.LogDebug("librdkafka[{Facility}] {Message}", m.Facility, m.Message))
                .SetPartitionsAssignedHandler((c, partitions) =>
                {
                    _logger.LogInformation("Partitions assigned: {Parts}",
                        string.Join(", ", partitions.Select(p => $"{p.Topic}[{p.Partition}]")));
                })
                .SetPartitionsRevokedHandler((c, partitions) =>
                {
                    _logger.LogWarning("Partitions revoked: {Parts}",
                        string.Join(", ", partitions.Select(p => $"{p.Topic}[{p.Partition}]")));
                })
                .Build();

            bool subscribed = false;
            while (!subscribed && !stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // Use the configured pattern, not a hardcoded one
                    consumer.Subscribe(new[] { pattern });
                    subscribed = true;
                    _logger.LogInformation("Kafka subscribed with pattern: {pattern}", pattern);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Kafka subscribe failed, retrying in 5 seconds...");
                    await Task.Delay(5000, stoppingToken);
                }
            }

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var cr = consumer.Consume(stoppingToken);

                    // Skip any topics that don’t match our pattern (extra safety)
                    if (!regex.IsMatch(cr.Topic))
                    {
                        _logger.LogTrace("Skipping non-matching topic={Topic}", cr.Topic);
                        continue;
                    }

                    _logger.LogDebug("Consumed topic={Topic} p={Partition} o={Offset} len={Len}",
                        cr.Topic, cr.Partition, cr.Offset, cr.Message.Value?.Length);

                    using var scope = _scopeFactory.CreateScope();
                    var processor = scope.ServiceProvider.GetRequiredService<INotificationProcessor>();
                    await processor.ProcessEventAsync(cr.Topic, cr.Message.Value!);

                    consumer.StoreOffset(cr);
                    consumer.Commit(cr);

                    _logger.LogInformation("Kafka message processed: {Topic} Offset={Offset}", cr.Topic, cr.Offset);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (ConsumeException ex)
                {
                    _logger.LogError(ex, "Kafka consume error");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Kafka worker unexpected error");
                }
            }

            consumer.Close();
        }
    }
}
