using FactoryOperation_KafkaMqttService.FactoryOpsApp.Messaging.Interfaces;
using System.Text.Json;

namespace FactoryOperation_KafkaMqttService.FactoryOpsApp.Messaging.Services
{
    public class MqttDlqPublisher : IMqttDlqPublisher
    {
        private readonly IMqttClientService _mqtt;
        public MqttDlqPublisher(IMqttClientService mqtt) => _mqtt = mqtt;

        public async Task PublishAsync(int tenantId, string originalTopic, byte[] payload, string reason, CancellationToken ct = default)
        {
            var dlqTopic = $"factory/{tenantId}/dlq";
            var envelope = JsonSerializer.SerializeToUtf8Bytes(new
            {
                originalTopic,
                payloadBase64 = Convert.ToBase64String(payload),
                reason,
                timestamp = DateTimeOffset.UtcNow
            });
            await _mqtt.PublishAsync(dlqTopic, envelope, qos: 1, retain: false, ct);
        }
    }
}
