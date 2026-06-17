using FactoryOperation_KafkaMqttService.FactoryOpsApp.Messaging.Interfaces;
using FactoryOperation_KafkaMqttService.FactoryOpsApp.Shared.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Text;

//namespace FactoryOperation_KafkaMqttService.Controllers.Messaging
namespace FactoryOperation_KafkaMqttService.Controllers.MqttKafkaController
{
    [ApiController]
    [Route("api/messaging")]
    public class MessagingManagementController : ControllerBase
    {
        private readonly IMqttClientService _mqtt;
        private readonly IKafkaProducerService _kafka;
        private readonly IMessagingSettingsProvider _settingsProvider;
        private readonly ILastTelemetryStore _telemetryStore;

        public MessagingManagementController(
            IMqttClientService mqtt,
            IKafkaProducerService kafka,
            IMessagingSettingsProvider settingsProvider,
            ILastTelemetryStore telemetryStore)
        {
            _mqtt = mqtt;
            _kafka = kafka;
            _settingsProvider = settingsProvider;
            _telemetryStore = telemetryStore;
        }

        // =====================================================
        // 1️⃣ SYSTEM STATUS
        // =====================================================
        [HttpGet("status")]
        public IActionResult GetSystemStatus()
        {
            return Ok(new
            {
                mqttConnected = _mqtt.IsConnected,
                timestamp = DateTime.UtcNow
            });
        }

        // =====================================================
        // 2️⃣ MQTT CONTROL
        // =====================================================
        [HttpPost("mqtt/start")]
        public async Task<IActionResult> StartMqtt()
        {
            await _mqtt.ConnectAsync();
            return Ok("MQTT Connected");
        }

        [HttpPost("mqtt/stop")]
        public async Task<IActionResult> StopMqtt()
        {
            await _mqtt.DisconnectAsync();
            return Ok("MQTT Disconnected");
        }

        [HttpPost("mqtt/restart")]
        public async Task<IActionResult> RestartMqtt()
        {
            await _mqtt.DisconnectAsync();
            await Task.Delay(1000);
            await _mqtt.ConnectAsync();
            return Ok("MQTT Restarted");
        }

        // =====================================================
        // 3️⃣ TEST MQTT PUBLISH
        // =====================================================
        [HttpPost("mqtt/publish-test")]
        public async Task<IActionResult> PublishTest(
            string topic,
            string message = "Hello from FactoryOps")
        {
            await _mqtt.PublishAsync(
                topic,
                Encoding.UTF8.GetBytes(message));

            return Ok("Message published");
        }

        // =====================================================
        // 4️⃣ KAFKA HEALTH CHECK
        // =====================================================
        [HttpPost("kafka/test")]
        public async Task<IActionResult> KafkaTest()
        {
            await _kafka.ProduceAsync(
                "factory-health-check",
                Guid.NewGuid().ToString(),
                Encoding.UTF8.GetBytes("ping"));

            return Ok("Kafka reachable");
        }

        // =====================================================
        // 5️⃣ CONFIG RELOAD
        // =====================================================
        [HttpPost("reload-config")]
        public async Task<IActionResult> ReloadConfig(int? tenantId = null)
        {
            await _settingsProvider.ReloadAsync(tenantId);

            return Ok(new
            {
                success = true,
                message = $"Configuration reloaded for tenant {tenantId}"
            });
        }

        // =====================================================
        // 6️⃣ LAST TELEMETRY API (Client Favorite)
        // =====================================================
        [HttpGet("telemetry/{deviceId}")]
        public IActionResult GetLastTelemetry(string deviceId)
        {
            var data = _telemetryStore.GetLastByDevice(deviceId);
            return Ok(data);
        }

        [HttpGet("telemetry-all")]
        public IActionResult GetAllTelemetry()
        {
            var data = _telemetryStore.GetLastAllDevices();
            return Ok(data);
        }
    }
}

