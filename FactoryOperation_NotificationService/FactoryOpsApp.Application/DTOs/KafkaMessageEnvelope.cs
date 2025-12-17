namespace FactoryOperation_WorkOrder.FactoryOpsApp.Application.DTOs
{
    public sealed class KafkaMessageEnvelope
    {
        public string Key { get; init; } = "";
        public object Payload { get; init; } = new { };
        public string Source { get; init; } = "MQTT";
        public DateTime Timestamp { get; init; } = DateTime.UtcNow;
        public IDictionary<string, string>? Headers { get; init; }
    }
}
