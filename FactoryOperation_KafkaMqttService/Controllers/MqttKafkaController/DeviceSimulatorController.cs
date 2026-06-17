using FactoryOperation_KafkaMqttService.FactoryOpsApp.Infrastructure.DBContext;
using FactoryOperation_KafkaMqttService.FactoryOpsApp.Messaging.Interfaces;
using FactoryOperation_KafkaMqttService.FactoryOpsApp.Shared.Interfaces;
using FactoryOperation_KafkaMqttService.FactoryOpsApp.Shared.Models;
using FactoryOps.Shared.Observability;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Text.Json;

namespace FactoryOperation_KafkaMqttService.Controllers.MqttKafkaController
{

    [ApiController]
    [Route("api/iot")]
    public class DeviceSimulatorController : ControllerBase
    {
        private readonly IMqttClientService _mqtt;
        private readonly TenantDbContextFactory _tenantDbFactory;
        private readonly ILogger<DeviceSimulatorController> _logger;
        private readonly ILastTelemetryStore _store;
        private readonly IKafkaProducerService _kafkaProducer;

        public DeviceSimulatorController(ILastTelemetryStore store,
            TenantDbContextFactory tenantDbFactory,
            IMqttClientService mqtt,
            ILogger<DeviceSimulatorController> logger,
            IKafkaProducerService kafkaProducer)
        {
            _mqtt = mqtt;
            _logger = logger;
            _tenantDbFactory = tenantDbFactory;
            _store = store;
            _kafkaProducer = kafkaProducer;
        }


        [HttpPost("connect")]
        public async Task<IActionResult> Connect(CancellationToken ct)
        {
            try
            {
                if (!_mqtt.IsConnected)
                    await _mqtt.ConnectAsync(ct);

                return Ok(new { connected = _mqtt.IsConnected });
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Connect attempt failed.");
                return StatusCode(502, new { connected = false, error = ex.Message });
            }
        }

        public record SimulateOnceRequest(
            string Topic,            // e.g. "tenant/1/devices/{deviceId}/telemetry"
            string DeviceId,         // e.g. "DEV001"
            int QoS = 1,
            bool Retain = false
        );

        [HttpPost("publish-random")]
        public async Task<IActionResult> PublishRandom([FromBody] SimulateOnceRequest req, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(req.Topic)) return BadRequest("Topic is required.");
            if (string.IsNullOrWhiteSpace(req.DeviceId)) return BadRequest("DeviceId is required.");

            if (!_mqtt.IsConnected)
                await _mqtt.ConnectAsync(ct);

            var topic = ExpandTopic(req.Topic, req.DeviceId);
            var payload = BuildRandomTelemetry(req.DeviceId);

            await _mqtt.PublishAsync(topic, payload, req.QoS, req.Retain, ct);

            return Accepted(new
            {
                topic,
                bytes = payload.Length,
                qos = req.QoS,
                retain = req.Retain
            });
        }

        public record BurstRequest(
            string TopicTemplate,    // e.g. "tenant/1/devices/{deviceId}/telemetry"
            IList<string> DeviceIds, // e.g. ["DEV001","DEV002"]
            int MessagesPerDevice = 10,
            int IntervalMs = 250,
            int QoS = 1,
            bool Retain = false
        );

        [HttpPost("publish-invalid")]
        public async Task<IActionResult> PublishInvalid(
       [FromQuery] string deviceId = "DEV001",
       CancellationToken ct = default)
        {
            var topic = $"tenant/1/devices/{deviceId}/telemetry";

            if (!_mqtt.IsConnected)
                await _mqtt.ConnectAsync(ct);

            // 🔥 Intentionally invalid payload
            var invalidPayload = Encoding.UTF8.GetBytes("INVALID_PAYLOAD_TEST");

            await _mqtt.PublishAsync(topic, invalidPayload, qos: 1, retain: false, ct);

            _logger.LogWarning("Invalid payload published to topic {Topic}", topic);

            return Ok(new
            {
                message = "Invalid payload sent successfully",
                topic
            });
        }

        [HttpPost("publish-burst")]
        public async Task<IActionResult> PublishBurst([FromBody] BurstRequest req, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(req.TopicTemplate)) return BadRequest("TopicTemplate is required.");
            if (req.DeviceIds is null || req.DeviceIds.Count == 0) return BadRequest("At least one DeviceId is required.");
            if (req.MessagesPerDevice <= 0) return BadRequest("MessagesPerDevice must be > 0.");

            if (!_mqtt.IsConnected)
                await _mqtt.ConnectAsync(ct);

            var total = 0;
            foreach (var deviceId in req.DeviceIds)
            {
                for (int i = 0; i < req.MessagesPerDevice; i++)
                {
                    ct.ThrowIfCancellationRequested();

                    var topic = ExpandTopic(req.TopicTemplate, deviceId);
                    var payload = BuildRandomTelemetry(deviceId);

                    await _mqtt.PublishAsync(topic, payload, req.QoS, req.Retain, ct);
                    total++;

                    if (req.IntervalMs > 0)
                        await Task.Delay(req.IntervalMs, ct);
                }
            }

            return Accepted(new
            {
                published = total,
                devices = req.DeviceIds.Count,
                perDevice = req.MessagesPerDevice
            });
        }

        [HttpGet("status")]
        public IActionResult Status()
        {
            return Ok(new { mqttConnected = _mqtt.IsConnected });
        }

        private static string ExpandTopic(string template, string deviceId)
        {
            // Supports placeholders: {deviceId} and {guid}
            return template
                .Replace("{deviceId}", deviceId, StringComparison.OrdinalIgnoreCase)
                .Replace("{guid}", Guid.NewGuid().ToString("N"), StringComparison.OrdinalIgnoreCase);
        }

        private static byte[] BuildRandomTelemetry(string deviceId)
        {
            var rnd = Random.Shared;
            var payloadObj = new
            {
                deviceId,
                tsUtc = DateTime.UtcNow,
                metrics = new
                {
                    temperatureC = Math.Round(rnd.NextDouble() * 40 + 5, 2), // 5..45
                    humidityPct = Math.Round(rnd.NextDouble() * 60 + 20, 2), // 20..80
                    pressureKpa = Math.Round(rnd.NextDouble() * 20 + 95, 2), // 95..115
                    vibration = Math.Round(rnd.NextDouble() * 5, 3),         // 0..5
                    voltage = Math.Round(rnd.NextDouble() * 24 + 12, 2)      // 12..36
                }
            };

            var json = JsonSerializer.Serialize(payloadObj);
            return Encoding.UTF8.GetBytes(json);
        }

        public record PublishByTopicIdDto(int TenantId, int TopicId, string DeviceCode, string Payload);
        public class PublishByDeviceIdDto
        {
            public int TenantId { get; set; }

            public int DeviceId { get; set; }

            public int TopicId { get; set; }

            public string Payload { get; set; } = string.Empty;
        }
        // POST /api/iot/publish-by-topicId
        [HttpPost("publish-by-topicId")]
        public async Task<IActionResult> PublishByTopicId([FromBody] PublishByTopicIdDto dto, CancellationToken ct)
        {
            await using var db = _tenantDbFactory.GetTenantDbContext(dto.TenantId);
            var topic = await db.FactoryMqttTopics
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.TopicId == dto.TopicId && t.TenantId == dto.TenantId && !t.IsDeleted, ct);

            if (topic is null) return NotFound("Topic not found for tenant.");

            // Allow placeholder replacement if your paths use {deviceCode}
            var mqttPath = topic.MqttPath?
                .Replace("{deviceCode}", dto.DeviceCode, StringComparison.OrdinalIgnoreCase)
                .Replace("{device}", dto.DeviceCode, StringComparison.OrdinalIgnoreCase)
                ?? string.Empty;

            if (string.IsNullOrWhiteSpace(mqttPath)) return BadRequest("Invalid MqttPath in DB.");

            if (!_mqtt.IsConnected) await _mqtt.ConnectAsync(ct);
            await _mqtt.PublishAsync(mqttPath, Encoding.UTF8.GetBytes(dto.Payload ?? string.Empty), qos: 1, retain: false, ct);
            return Accepted(new { topic = mqttPath });
        }

        // POST /api/iot/publish-by-deviceId
        [HttpPost("publish-by-deviceId")]
        public async Task<IActionResult> PublishByDeviceId(
            [FromBody] PublishByDeviceIdDto dto,
            CancellationToken ct)
        {
            await using var db = _tenantDbFactory.GetTenantDbContext(dto.TenantId);

            // 1️⃣ Get Device
            var device = await db.FactoryDevices
                .AsNoTracking()
                .FirstOrDefaultAsync(d =>
                    d.DeviceId == dto.DeviceId &&
                    d.TenantId == dto.TenantId &&
                    !d.IsDeleted,
                    ct);

            if (device == null)
                return NotFound("Device not found.");

            // 2️⃣ Validate DeviceTopic Mapping
            var deviceTopic = await db.FactoryDeviceTopics
                .Include(x => x.Topic)
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.DeviceId == dto.DeviceId &&
                    x.TopicId == dto.TopicId &&
                    x.TenantId == dto.TenantId &&
                    x.IsActive &&
                    !x.IsDeleted,
                    ct);

            if (deviceTopic == null)
                return NotFound("Device is not mapped to this topic.");

            var topic = deviceTopic.Topic;

            if (topic == null || string.IsNullOrWhiteSpace(topic.MqttPath))
                return BadRequest("Invalid MQTT topic configuration.");

            // 3️⃣ Replace placeholders
            var mqttPath = topic.MqttPath
                .Replace("{TenantId}", dto.TenantId.ToString(), StringComparison.OrdinalIgnoreCase)
                .Replace("{deviceCode}", device.DeviceCode, StringComparison.OrdinalIgnoreCase)
                .Replace("{device}", device.DeviceCode, StringComparison.OrdinalIgnoreCase);

            // 4️⃣ Publish to MQTT
            if (!_mqtt.IsConnected)
                await _mqtt.ConnectAsync(ct);

            await _mqtt.PublishAsync(
                mqttPath,
                Encoding.UTF8.GetBytes(dto.Payload ?? string.Empty),
                qos: topic.QoS,
                retain: false,
                ct);

            return Accepted(new
            {
                deviceId = device.DeviceId,
                deviceCode = device.DeviceCode,
                topicId = topic.TopicId,
                topic = mqttPath
            });
        }

        [HttpGet("last5")]
        public IActionResult GetLastFive([FromQuery] string deviceId, [FromQuery] int take = 5, [FromQuery] bool decode = false)
        {
            if (string.IsNullOrWhiteSpace(deviceId))
                return BadRequest("deviceId is required.");

            var items = _store.GetLastByDevice(deviceId, take);
            var shaped = items.Select(m =>
            {
                var b64 = Convert.ToBase64String(m.Payload ?? Array.Empty<byte>());
                return new
                {
                    topic = m.Topic,
                    payloadBase64 = b64,
                    decodedPayload = decode ? TryDecodeUtf8Json(m.Payload) : null,
                    qos = m.QoS,
                    retain = m.Retain,
                    receivedAt = m.ReceivedAt
                };
            });

            return Ok(shaped);
        }

        [HttpGet("last5-all")]
        public IActionResult GetLastFiveAll([FromQuery] int take = 5, [FromQuery] bool decode = false)
        {
            var map = _store.GetLastAllDevices(take);
            var shaped = map.ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value.Select(m =>
                {
                    var b64 = Convert.ToBase64String(m.Payload ?? Array.Empty<byte>());
                    return new
                    {
                        topic = m.Topic,
                        payloadBase64 = b64,
                        decodedPayload = decode ? TryDecodeUtf8Json(m.Payload) : null,
                        qos = m.QoS,
                        retain = m.Retain,
                        receivedAt = m.ReceivedAt
                    };
                }).ToArray());

            return Ok(shaped);
        }

        private static object? TryDecodeUtf8Json(byte[]? bytes)
        {
            if (bytes is null || bytes.Length == 0) return null;
            try
            {
                var json = Encoding.UTF8.GetString(bytes);
                using var doc = JsonDocument.Parse(json);
                return JsonSerializer.Deserialize<object>(json);
            }
            catch
            {
                // fallback to UTF8 text if not JSON
                try { return Encoding.UTF8.GetString(bytes); } catch { return null; }
            }
        }

        [HttpPost("ping")]
        public IActionResult Ping()
        {
            AppMetrics.RequestCounter.Add(1);

            return Ok("Metric emitted");
        }


        [HttpPost("kafka-test")]
        public async Task<IActionResult> KafkaToMqttTest()
        {
            var messageId = Guid.NewGuid().ToString("N");

            var envelope = new KafkaMessageEnvelope
            {
                MessageId = messageId,
                CorrelationId = messageId,

                EventType = "TestEvent",
                Producer = "ManualTest",

                TenantId = 78,

                Key = $"78_DEV99_{messageId}",

                Payload = new
                {
                    payloadBase64 = Convert.ToBase64String(
                        Encoding.UTF8.GetBytes("Hello from Kafka Test"))
                },

                Headers = new Dictionary<string, string>
                {
                    ["tenant-id"] = "78",
                    ["device-code"] = "DEV99",
                    ["event-type"] = "TestEvent"
                }
            };

            await _kafkaProducer.ProduceEnvelopeAsync(
                "factoryops.78.devices.DEV99.telemetry",
                envelope);

            return Ok("Kafka message sent");
        }

    }
}
