namespace FactoryOperation_KafkaMqttService.FactoryOpsApp.Messaging.Models
{
    public sealed class MqttMessage
    {
        public string Topic { get; init; } = "";
        public byte[] Payload { get; init; } = Array.Empty<byte>();
        public int QoS { get; init; } = 1;
        public bool Retain { get; init; } = false;
        public DateTimeOffset ReceivedAt { get; init; } = DateTimeOffset.UtcNow;
    }
}
