using FactoryOperation_KafkaMqttService.FactoryOpsApp.Messaging.Interfaces;
using FactoryOperation_KafkaMqttService.FactoryOpsApp.Messaging.Models;
using FactoryOperation_KafkaMqttService.FactoryOpsApp.Shared.Config;
using FactoryOperation_KafkaMqttService.FactoryOpsApp.Shared.Interfaces;
using FactoryOperation_KafkaMqttService.FactoryOpsApp.Shared.Models;
using FactoryOps.Shared.Observability;
using System.Text;
using System.Text.Json;
using System.Threading.Channels;

namespace FactoryOperation_KafkaMqttService.FactoryOpsApp.Messaging.Services
{
    /// <summary>
    /// Infrastructure-only bridge service.
    /// Responsible for translating MQTT ↔ Kafka messages.
    /// Contains NO domain or business logic.
    /// </summary>
    public sealed class MqttToKafkaBridgeService : BackgroundService
    {
        private readonly IMqttClientService _mqtt;
        private readonly IKafkaProducerService _kafka;
        private readonly IKafkaConsumerBridgeService _kafkaConsumer;
        private readonly IMessagingSettingsProvider _settingsProvider;
        private readonly ITenantResolver _tenantResolver;
        private readonly ILogger<MqttToKafkaBridgeService> _logger;
        private readonly Channel<MqttMessage> _channel;
        private readonly ILastTelemetryStore _lastStore;
        private string? _currentMqttTopic;

        private int _backlogDepth;
        private int _kafkaToMqttLoopStarted = 0;

        public MqttToKafkaBridgeService(
            IMqttClientService mqtt,
            IKafkaProducerService kafka,
            IKafkaConsumerBridgeService kafkaConsumer,
            IMessagingSettingsProvider settingsProvider,
            ITenantResolver tenantResolver,
            ILastTelemetryStore lastStore,
            ILogger<MqttToKafkaBridgeService> logger)
        {
            _mqtt = mqtt;
            _kafka = kafka;
            _kafkaConsumer = kafkaConsumer;
            _settingsProvider = settingsProvider;
            _tenantResolver = tenantResolver;
            _lastStore = lastStore;
            _logger = logger;

            _channel = Channel.CreateBounded<MqttMessage>(new BoundedChannelOptions(20_000)
            {
                SingleReader = false,
                SingleWriter = false,
                FullMode = BoundedChannelFullMode.Wait
            });

            _settingsProvider.SettingsChanged += OnSettingsChanged;
        }

        // SETTINGS HOT RELOAD (SAFE)
        private void OnSettingsChanged(object? sender, int? tenantId)
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    var settings = _settingsProvider.GetEffectiveSettings(tenantId);
                    var newTopic = settings.Mqtt.Topic;

                    if (string.Equals(_currentMqttTopic, newTopic, StringComparison.OrdinalIgnoreCase))
                        return;

                    _logger.LogInformation(
                        "Bridge MQTT topic changed: {Old} → {New}",
                        _currentMqttTopic,
                        newTopic);

                    if (!string.IsNullOrWhiteSpace(_currentMqttTopic))
                    {
                        await _mqtt.UnsubscribeAsync(_currentMqttTopic);
                    }

                    await _mqtt.SubscribeAsync(newTopic, async msg =>
                    {
                        _lastStore.Add(msg);
                        await _channel.Writer.WriteAsync(msg);
                        Interlocked.Increment(ref _backlogDepth);
                    });

                    _currentMqttTopic = newTopic;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to reload bridge settings");
                }
            });
        }

        // SERVICE LOOP
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Telemetry.RegisterBridgeQueueDepth(() =>
                Volatile.Read(ref _backlogDepth));

            await _mqtt.ConnectAsync(stoppingToken);

            var globalSettings = _settingsProvider.GetEffectiveSettings(null);
            _currentMqttTopic = "tenant/+/devices/#";

            await _mqtt.SubscribeAsync(_currentMqttTopic, async msg =>
            {
                _lastStore.Add(msg);
                await _channel.Writer.WriteAsync(msg, stoppingToken);
                Interlocked.Increment(ref _backlogDepth);
            }, stoppingToken);

            _logger.LogInformation(
                "MQTT→Kafka bridge started (mqttFilter={Filter})",
                globalSettings.Mqtt.Topic);

            // Start Kafka→MQTT loop ONCE
            if (globalSettings.Kafka.EnableKafkaToMqtt &&
                Interlocked.Exchange(ref _kafkaToMqttLoopStarted, 1) == 0)
            {
                _ = Task.Run(
                    () => RunKafkaToMqttLoop(stoppingToken),
                    stoppingToken);

                _logger.LogInformation(
                    "Kafka→MQTT bridge enabled (kafkaTopic={KafkaTopic})",
                    globalSettings.Kafka.KafkaToMqttTopic ?? globalSettings.Kafka.Topic);
            }

            // WORKER LOOP
            var workerCount = Environment.ProcessorCount; // auto scale

            _logger.LogInformation("Starting {WorkerCount} bridge workers", workerCount);

            var tasks = new List<Task>();

            for (int i = 0; i < workerCount; i++)
            {
                tasks.Add(Task.Run(() => ProcessChannel(stoppingToken), stoppingToken));
            }

            await Task.WhenAll(tasks);
        }

        private async Task ProcessChannel(CancellationToken ct)
        {
            await foreach (var msg in _channel.Reader.ReadAllAsync(ct))
            {
                try
                {
                    await ForwardToKafkaAsync(msg, ct);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Bridge processing failed");
                    Telemetry.KafkaProduceErrors.Add(1);
                    try
                    {
                        await PublishToKafkaDlqAsync("bridge-processing-error", msg, ct);
                    }
                    catch { }
                }
                finally
                {
                    Interlocked.Decrement(ref _backlogDepth);
                }
            }
        }

        // MQTT → Kafka (DYNAMIC TENANT CONFIG)
        //private async Task ForwardToKafkaAsync(MqttMessage msg, CancellationToken ct)
        //{
        //    var tenantId = _tenantResolver.ResolveFromMqtt(msg.Topic);
        //    var deviceCode = ExtractDeviceCodeFromTopic(msg.Topic);

        //    var settings = _settingsProvider.GetEffectiveSettings(tenantId);

        //    if (!tenantId.HasValue)
        //    {
        //        await PublishToKafkaDlqAsync("missing-tenant-id", msg, ct);
        //        return;
        //    }
        //    var action = DetectActionFromMqttTopic(msg.Topic);
        //    var kafkaTopic = ChooseKafkaTopic(settings.Kafka, tenantId, deviceCode, action);

        //    var envelope = new KafkaMessageEnvelope
        //    {
        //        EventType = "DeviceTelemetryReceived",
        //        EventVersion = "v1",
        //        Producer = "FactoryOpsKafkaMqttService",
        //        Source = "MQTT",
        //        OccurredAt = msg.ReceivedAt.UtcDateTime,
        //        Timestamp = DateTime.UtcNow,

        //        TenantId = tenantId,
        //        CorrelationId = Guid.NewGuid().ToString("N"),

        //        Key = $"{tenantId}_{deviceCode ?? "unknown"}_{Guid.NewGuid():N}",

        //        Payload = new
        //        {
        //            tenantId,
        //            deviceCode,
        //            mqttTopic = msg.Topic,
        //            payloadBase64 = Convert.ToBase64String(msg.Payload),
        //            qos = msg.QoS,
        //            receivedAt = msg.ReceivedAt
        //        },

        //        Headers = new Dictionary<string, string>
        //        {
        //            ["tenant-id"] = tenantId.Value.ToString(),
        //            ["device-code"] = deviceCode ?? "unknown",
        //            ["event-type"] = "DeviceTelemetryReceived"
        //        }
        //    };

        //    await _kafka.ProduceEnvelopeAsync(kafkaTopic, envelope, ct);

        //    if (msg.ReceivedAt != default)
        //    {
        //        var latencyMs = (DateTimeOffset.UtcNow - msg.ReceivedAt).TotalMilliseconds;
        //        Telemetry.BridgeLatencyMs.Record(
        //            latencyMs,
        //            new KeyValuePair<string, object?>[]
        //            {
        //            new("kafka_topic", kafkaTopic)
        //            });
        //    }
        //}

        private async Task ForwardToKafkaAsync(MqttMessage msg, CancellationToken ct)
        {
            var tenantId = _tenantResolver.ResolveFromMqtt(msg.Topic);

            Telemetry.MqttMessagesReceived.Add(1);
            Telemetry.MqttBytesReceived.Add(msg.Payload?.Length ?? 0);

            var deviceCode = ExtractDeviceCodeFromTopic(msg.Topic);

            if (!tenantId.HasValue)
            {
                await PublishToKafkaDlqAsync("missing-tenant-id", msg, ct);
                return;
            }

            // LOOP PREVENTION + LOG
            if (IsKafkaOrigin(msg.Payload!))
            {
                _logger.LogDebug("Skipping Kafka-origin message (loop prevention)");
                return;
            }

            if (!TryValidateJsonPayload(msg.Payload!, out var validationError))
            {
                Telemetry.MqttPublishErrors.Add(1);
                _logger.LogWarning(
                    "Invalid JSON payload from topic {Topic}: {Error}",
                    msg.Topic,
                    validationError);

                await PublishToKafkaDlqAsync("invalid-json-payload", msg, ct);
                return;
            }

            var settings = _settingsProvider.GetEffectiveSettings(tenantId);
            var action = DetectActionFromMqttTopic(msg.Topic);

            var kafkaTopic = ChooseKafkaTopic(
                settings.Kafka,
                tenantId,
                deviceCode,
                action);

            // CORE FIX: SAME ID FLOW
            using var doc = JsonDocument.Parse(msg.Payload);

            var root = doc.RootElement;

            var messageId =
                root.TryGetProperty("messageId", out var mid)
                    ? mid.GetString()
                    : Guid.NewGuid().ToString("N");

            var correlationId =
                root.TryGetProperty("correlationId", out var cid)
                    ? cid.GetString()
                    : messageId;

            var envelope = new KafkaMessageEnvelope
            {
                MessageId = messageId!,
                CorrelationId = correlationId, // SAME FLOW

                EventType = "DeviceTelemetryReceived",
                EventVersion = "v1",
                Producer = "FactoryOpsKafkaMqttService",
                Source = "MQTT",

                OccurredAt = msg.ReceivedAt.UtcDateTime,
                Timestamp = DateTime.UtcNow,

                TenantId = tenantId,
                Key = $"{tenantId}_{deviceCode ?? "unknown"}_{messageId}",

                Payload = new
                {
                    tenantId,
                    deviceCode,
                    mqttTopic = msg.Topic,
                    payloadBase64 = Convert.ToBase64String(msg.Payload!),
                    qos = msg.QoS,
                    receivedAt = msg.ReceivedAt
                },

                Headers = new Dictionary<string, string>
                {
                    ["tenant-id"] = tenantId.Value.ToString(),
                    ["device-code"] = deviceCode ?? "unknown",
                    ["event-type"] = "DeviceTelemetryReceived",
                    [BridgeHeaders.Origin] = BridgeHeaders.Mqtt,

                    // TRACE HEADERS
                    ["message-id"] = messageId!,
                    ["correlation-id"] = correlationId!
                }
            };

            await _kafka.ProduceEnvelopeAsync(kafkaTopic, envelope, ct);
            Telemetry.KafkaMessagesProduced.Add(1);
            // LOGGING (CLIENT PROOF)
            _logger.LogInformation(
                "MQTT→Kafka | MessageId={MessageId} Tenant={Tenant} Device={Device}",
                messageId,
                tenantId,
                deviceCode);

            // existing metric preserved
            if (msg.ReceivedAt != default)
            {
                var latencyMs = (DateTimeOffset.UtcNow - msg.ReceivedAt).TotalMilliseconds;
                Telemetry.BridgeLatencyMs.Record(
                    latencyMs,
                    new KeyValuePair<string, object?>[]
                    {
                new("kafka_topic", kafkaTopic)
                    });
            }
        }
        // Kafka → MQTT LOOP (TENANT-AWARE)
        private async Task RunKafkaToMqttLoop(CancellationToken ct)
        {
            await _kafkaConsumer.ConsumeAsync(async envelope =>
            {
                if (envelope.Headers != null &&
                    envelope.Headers.TryGetValue(BridgeHeaders.Origin, out var origin) &&
                    origin == BridgeHeaders.Mqtt)
                {
                    return;
                }

                try
                {
                    var tenantId = _tenantResolver.ResolveFromKafka(envelope);

                    var settings = _settingsProvider.GetEffectiveSettings(tenantId);

                    var mqttTopic = MapKafkaToMqttTopic(
                        envelope,
                        settings.Mqtt.Topic);

                    if (string.IsNullOrWhiteSpace(mqttTopic))
                    {
                        _logger.LogWarning(
                            "Kafka→MQTT skipped: no topic mapping for key={Key}",
                            envelope.Key);
                        return;
                    }
                    string? payloadBase64 = null;

                    if (envelope.Payload is JsonElement json &&
                        json.TryGetProperty("payloadBase64", out var payloadElement))
                    {
                        payloadBase64 = payloadElement.GetString();
                    }

                    if (string.IsNullOrEmpty(payloadBase64))
                        throw new InvalidOperationException("Missing payloadBase64 in Kafka message");

                    var payload = Convert.FromBase64String(payloadBase64);

                    // TRACE FIX
                    var messageId = envelope.MessageId ?? Guid.NewGuid().ToString("N");
                    var correlationId = envelope.CorrelationId ?? messageId;

                    var wrappedPayload = JsonSerializer.Serialize(new
                    {
                        origin = BridgeHeaders.Kafka,
                        messageId = messageId,
                        correlationId = correlationId,
                        tenantId = tenantId,
                        ts = DateTime.UtcNow,
                        data = Convert.ToBase64String(payload)
                    });

                    _logger.LogDebug(
                        "Publishing Kafka-origin message to MQTT | MessageId={MessageId}",
                        messageId);

                    _logger.LogWarning(
                        "Kafka→MQTT PUBLISHING | Topic={Topic} MessageId={MessageId}",
                        mqttTopic,
                        messageId);

                    await _mqtt.PublishAsync(
                        mqttTopic,
                        Encoding.UTF8.GetBytes(wrappedPayload),
                        qos: settings.Mqtt.QoS,
                        retain: false,
                        ct);

                    Telemetry.BridgeKafkaToMqttCounter.Add(1);

                    // 🔥 LOGGING
                    _logger.LogInformation(
                        "Kafka→MQTT | MessageId={MessageId} Tenant={Tenant}",
                        messageId,
                        tenantId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Kafka→MQTT bridge failed");

                    try
                    {
                        Telemetry.BridgeKafkaToMqttErrors.Add(1);
                    }
                    catch { }

                    await PublishKafkaToMqttDlqAsync(
                        "kafka-to-mqtt-failure",
                        envelope,
                        ct);
                }
            }, ct);
        }

        // DLQ: MQTT → Kafka
        private async Task PublishToKafkaDlqAsync(
            string reason,
            MqttMessage msg,
            CancellationToken ct)
        {
            var settings = _settingsProvider.GetEffectiveSettings(null);

            if (!(settings.Kafka.Dlq?.Enabled ?? true))
            {
                _logger.LogWarning(
                    "DLQ disabled. Dropping MQTT message topic={Topic}, reason={Reason}",
                    msg.Topic,
                    reason);
                return;
            }

            var dlqTopic = settings.Kafka.Dlq?.Topic
                           ?? $"{settings.Kafka.Topic}-dlq";

            var envelope = new KafkaMessageEnvelope
            {
                EventType = "BridgeDlqEvent",
                EventVersion = "v1",
                Producer = "FactoryOpsKafkaMqttService",
                Source = "MQTT-DLQ",
                Timestamp = DateTime.UtcNow,

                Key = $"dlq_{Guid.NewGuid():N}",

                Payload = new
                {
                    reason,
                    mqttTopic = msg.Topic,
                    payloadBase64 = Convert.ToBase64String(msg.Payload),
                    qos = msg.QoS,
                    receivedAt = msg.ReceivedAt,
                    routedAt = DateTime.UtcNow
                },

                Headers = new Dictionary<string, string>
                {
                    ["x-error"] = reason,
                    ["x-original-topic"] = msg.Topic
                }
            };

            await _kafka.ProduceEnvelopeAsync(dlqTopic, envelope, ct);
            Telemetry.MqttPublishErrors.Add(1);
            _logger.LogWarning(
                "Routed MQTT message to Kafka DLQ topic={DlqTopic}, reason={Reason}",
                dlqTopic,
                reason);
        }

        // DLQ: Kafka → MQTT
        private async Task PublishKafkaToMqttDlqAsync(
            string reason,
            KafkaMessageEnvelope envelope,
            CancellationToken ct)
        {
            var settings = _settingsProvider.GetEffectiveSettings(
                _tenantResolver.ResolveFromKafka(envelope));

            if (!(settings.Kafka.Dlq?.Enabled ?? true))
                return;

            var dlqTopic = settings.Kafka.Dlq?.Topic
                           ?? $"{settings.Kafka.Topic}-dlq";

            var dlqEnvelope = new KafkaMessageEnvelope
            {
                EventType = "BridgeDlqEvent",
                EventVersion = "v1",
                Producer = "FactoryOpsKafkaMqttService",
                Source = "KAFKA-DLQ",
                Timestamp = DateTime.UtcNow,

                Key = $"dlq_{Guid.NewGuid():N}",

                Payload = new
                {
                    reason,
                    kafkaKey = envelope.Key,
                    headers = envelope.Headers,
                    payload = envelope.Payload,
                    routedAt = DateTime.UtcNow
                },

                Headers = new Dictionary<string, string>
                {
                    ["x-error"] = reason,
                    ["x-bridge-direction"] = "kafka-to-mqtt"
                }
            };

            await _kafka.ProduceEnvelopeAsync(dlqTopic, dlqEnvelope, ct);
        }

        // HELPERS
        private static string MapKafkaToMqttTopic(
            KafkaMessageEnvelope envelope,
            string defaultPattern)
        {
            if (envelope.Headers == null)
                return string.Empty;

            if (!envelope.Headers.TryGetValue("tenant-id", out var tenantId))
                return string.Empty;

            if (!envelope.Headers.TryGetValue("device-code", out var deviceCode))
                return string.Empty;

            // Use bridge pattern from DB (already applied to mqtt.Topic)
            return defaultPattern
                .Replace("{tenantId}", tenantId)
                .Replace("{deviceCode}", deviceCode);
        }

        private static string ChooseKafkaTopic(
            KafkaSettings kafkaSettings,
            int? tenantId,
            string? deviceCode,
            string action)
        {
            if (!tenantId.HasValue)
                throw new InvalidOperationException("TenantId is required to build Kafka topic");

            if (string.IsNullOrWhiteSpace(deviceCode))
                deviceCode = "unknown";

            if (string.IsNullOrWhiteSpace(action))
                action = "telemetry"; // safe default

            // Format:
            // factoryops/{tenantId}/devices/{deviceCode}/{action}
            return $"factoryops.{tenantId.Value}.devices.{deviceCode}.{action}";
        }



        private static string? ExtractDeviceCodeFromTopic(string topic)
        {
            var parts = topic.Split('/', StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < parts.Length - 1; i++)
            {
                if ((parts[i].Equals("device", StringComparison.OrdinalIgnoreCase)
                    || parts[i].Equals("devices", StringComparison.OrdinalIgnoreCase)))
                {
                    return parts[i + 1];
                }
            }
            return null;
        }

        private static string DetectActionFromMqttTopic(string mqttTopic)
        {
            mqttTopic = mqttTopic.ToLowerInvariant();

            if (mqttTopic.Contains("/status")) return "status";
            if (mqttTopic.Contains("/alerts")) return "alerts";
            if (mqttTopic.Contains("/config")) return "config";
            if (mqttTopic.Contains("/command")) return "commands";

            return "telemetry"; // default
        }

        private static bool TryValidateJsonPayload(
    byte[] payload,
    out string? error)
        {
            try
            {
                var json = Encoding.UTF8.GetString(payload);

                using var doc = JsonDocument.Parse(json);

                error = null;
                return true;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return false;
            }
        }

        private static bool IsKafkaOrigin(byte[] payload)
        {
            try
            {
                var json = Encoding.UTF8.GetString(payload);

                using var doc = JsonDocument.Parse(json);

                if (doc.RootElement.TryGetProperty("origin", out var origin))
                {
                    return origin.GetString() == BridgeHeaders.Kafka;
                }
            }
            catch
            {
            }

            return false;
        }

    }
}
