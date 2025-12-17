using FactoryOperation_KafkaMqttService.FactoryOpsApp.Infrastructure.DBContext;
using FactoryOperation_KafkaMqttService.FactoryOpsApp.Messaging.Config;
using FactoryOperation_KafkaMqttService.FactoryOpsApp.Messaging.Interfaces;
using FactoryOperation_KafkaMqttService.FactoryOpsApp.Messaging.Models;
using FactoryOperation_KafkaMqttService.FactoryOpsApp.Shared.Config;
using FactoryOperation_KafkaMqttService.FactoryOpsApp.Shared.Interfaces;
using FactoryOperation_KafkaMqttService.FactoryOpsApp.Shared.Models;
using FactoryOpsApp.Domain.Entities.FactoryOpsTenants;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Threading.Channels;

namespace FactoryOperation_KafkaMqttService.FactoryOpsApp.Messaging.Services
{

    public class MqttToKafkaBridgeService : BackgroundService
    {
        private readonly IMqttClientService _mqtt;
        private readonly IKafkaProducerService _kafka;
        private readonly KafkaSettings _kafkaSettings;
        private readonly MqttSettings _mqttSettings;
        private readonly ILogger<MqttToKafkaBridgeService> _logger;
        private readonly Channel<MqttMessage> _channel;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILastTelemetryStore _lastStore;

        private int _backlogDepth; // step 4: backlog gauge

        public MqttToKafkaBridgeService(
            IMqttClientService mqtt,
            IKafkaProducerService kafka,
            IOptions<KafkaSettings> kafkaOpts,
            IOptions<MqttSettings> mqttOpts,
            IServiceScopeFactory scopeFactory,
            ILastTelemetryStore lastStore,
            ILogger<MqttToKafkaBridgeService> logger)
        {
            _mqtt = mqtt;
            _kafka = kafka;
            _kafkaSettings = kafkaOpts.Value;
            _mqttSettings = mqttOpts.Value;
            _scopeFactory = scopeFactory;
            _lastStore = lastStore;
            _logger = logger;
            _channel = Channel.CreateBounded<MqttMessage>(new BoundedChannelOptions(20_000)
            {
                SingleReader = true,
                SingleWriter = false,
                FullMode = BoundedChannelFullMode.Wait
            });
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // step 4: expose backlog depth to OTel as an observable gauge
            Observability.Telemetry.RegisterBridgeQueueDepth(() => Volatile.Read(ref _backlogDepth));

            await _mqtt.ConnectAsync(stoppingToken);

            await _mqtt.SubscribeAsync(_mqttSettings.Topic, async (m) =>
            {
                // Store latest per-device for UI/APIs
                _lastStore.Add(m);

                await _channel.Writer.WriteAsync(m, stoppingToken);
                Interlocked.Increment(ref _backlogDepth); // step 4: increase backlog after enqueued
            }, stoppingToken);

            _logger.LogInformation("MqttToKafkaBridgeService started (mqttFilter={Filter}, kafkaTopic={Topic})",
                _mqttSettings.Topic, _kafkaSettings.Topic);

            await foreach (var msg in _channel.Reader.ReadAllAsync(stoppingToken))
            {
                try
                {
                    var tenantId = ExtractTenantIdFromTopic(msg.Topic);
                    var deviceCode = ExtractDeviceCodeFromTopic(msg.Topic);

                    // step 2: subscriber-side DLQ if we cannot resolve tenant
                    if (!tenantId.HasValue)
                    {
                        await PublishToKafkaDlqAsync("missing-tenant-id", msg, stoppingToken);
                        continue;
                    }

                    if (!string.IsNullOrEmpty(deviceCode))
                    {
                        await UpdateDeviceHeartbeatAsync(tenantId.Value, deviceCode);
                    }

                    var envelope = new KafkaMessageEnvelope
                    {
                        Key = $"{tenantId ?? 0}_{deviceCode ?? "unknown"}_{Guid.NewGuid():N}",
                        Payload = new
                        {
                            tenantId = tenantId,
                            deviceCode = deviceCode,
                            topic = msg.Topic,
                            payloadBase64 = Convert.ToBase64String(msg.Payload),
                            qos = msg.QoS,
                            receivedAt = msg.ReceivedAt
                        },
                        Source = "MQTT",
                        Timestamp = DateTime.UtcNow,
                        Headers = new Dictionary<string, string>
                        {
                            ["tenant-id"] = (tenantId?.ToString() ?? "0"),
                            ["device-code"] = deviceCode ?? "unknown"
                        }
                    };

                    var kafkaTopic = ChooseKafkaTopic(tenantId);
                    await _kafka.ProduceEnvelopeAsync(kafkaTopic, envelope, stoppingToken);

                    // step 4: end-to-end latency metric
                    try
                    {
                        if (msg.ReceivedAt != default)
                        {
                            var ms = (DateTimeOffset.UtcNow - msg.ReceivedAt).TotalMilliseconds;
                            Observability.Telemetry.BridgeLatencyMs.Record(ms,
                                new KeyValuePair<string, object?>("topic", msg.Topic),
                                new KeyValuePair<string, object?>("kafka_topic", kafkaTopic));
                        }
                    }
                    catch { }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to forward MQTT message to Kafka");
                    // step 2: route to DLQ on processing exception
                    try { await PublishToKafkaDlqAsync("processing-exception", msg, stoppingToken); } catch { /* best effort */ }
                }
                finally
                {
                    Interlocked.Decrement(ref _backlogDepth); // step 4: decrease backlog after processed
                }
            }
        }

        private async Task PublishToKafkaDlqAsync(string reason, MqttMessage msg, CancellationToken ct)
        {
            if (!(_kafkaSettings.Dlq?.Enabled ?? true))
            {
                _logger.LogWarning("DLQ disabled; dropping message for topic {Topic} reason {Reason}", msg.Topic, reason);
                return;
            }

            var dlqTopic = _kafkaSettings.Dlq?.Topic ?? $"{_kafkaSettings.Topic}-dlq";
            var envelope = new KafkaMessageEnvelope
            {
                Key = $"dlq_{Guid.NewGuid():N}",
                Payload = new
                {
                    reason,
                    topic = msg.Topic,
                    payloadBase64 = Convert.ToBase64String(msg.Payload),
                    qos = msg.QoS,
                    receivedAt = msg.ReceivedAt,
                    routedAt = DateTime.UtcNow
                },
                Source = "MQTT-DLQ",
                Timestamp = DateTime.UtcNow,
                Headers = new Dictionary<string, string>
                {
                    ["x-error"] = reason,
                    ["x-original-topic"] = msg.Topic
                }
            };

            await _kafka.ProduceEnvelopeAsync(dlqTopic, envelope, ct);
            _logger.LogWarning("Routed message to Kafka DLQ {DlqTopic} for topic {Topic} reason {Reason}", dlqTopic, msg.Topic, reason);
        }

        private string ChooseKafkaTopic(int? tenantId)
        {
            if (_kafkaSettings.UsePerTenantTopic && tenantId.HasValue && tenantId.Value > 0)
            {
                return $"tenant-{tenantId.Value}-telemetry";
            }
            return _kafkaSettings.Topic;
        }

        private static int? ExtractTenantIdFromTopic(string topic)
        {
            if (string.IsNullOrWhiteSpace(topic)) return null;
            var parts = topic.Split('/', StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < parts.Length - 1; i++)
            {
                if (parts[i].Equals("tenant", StringComparison.OrdinalIgnoreCase) && i + 1 < parts.Length)
                {
                    if (int.TryParse(parts[i + 1], out var id))
                        return id;
                }
            }
            return null;
        }

        private static string? ExtractDeviceCodeFromTopic(string topic)
        {
            if (string.IsNullOrWhiteSpace(topic)) return null;
            var parts = topic.Split('/', StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < parts.Length - 1; i++)
            {
                if ((parts[i].Equals("devices", StringComparison.OrdinalIgnoreCase) ||
                     parts[i].Equals("device", StringComparison.OrdinalIgnoreCase))
                    && i + 1 < parts.Length)
                {
                    return parts[i + 1];
                }
            }
            return null;
        }

        private async Task UpdateDeviceHeartbeatAsync(int tenantId, string deviceCode)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var dbFactory = scope.ServiceProvider.GetRequiredService<TenantDbContextFactory>();
                await using var db = dbFactory.GetTenantDbContext(tenantId);

                var device = await db.FactoryDevices
                    .FirstOrDefaultAsync(d => d.DeviceCode == deviceCode && d.TenantId == tenantId);

                if (device != null)
                {
                    device.LastSeen = DateTime.UtcNow;
                    device.Status = DeviceStatusEnum.Online;
                    await db.SaveChangesAsync();
                    _logger.LogDebug("Updated device {DeviceCode} status for tenant {TenantId}", deviceCode, tenantId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to update device heartbeat for tenant {TenantId} device {DeviceCode}", tenantId, deviceCode);
            }
        }
    }
}
