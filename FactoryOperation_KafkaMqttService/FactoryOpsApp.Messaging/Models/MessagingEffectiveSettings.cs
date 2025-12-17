
using FactoryOperation_KafkaMqttService.FactoryOpsApp.Messaging.Config;
using FactoryOperation_KafkaMqttService.FactoryOpsApp.Shared.Config;

namespace FactoryOperation_KafkaMqttService.FactoryOpsApp.Messaging.Models
{
    public sealed class MessagingEffectiveSettings
    {
        public MqttSettings Mqtt { get; init; } = new();
        public KafkaSettings Kafka { get; init; } = new();
        public int TenantId { get; init; } = 0;
        public DateTime LoadedAtUtc { get; init; } = DateTime.UtcNow;
        public string Source { get; init; } = "db-cache";
    }
}
