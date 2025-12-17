namespace FactoryOperation_KafkaMqttService.FactoryOpsApp.Shared.Models
{
    //public sealed class KafkaMessageEnvelope
    //{
    //    public string Key { get; set; } = default!;
    //    public string Version { get; set; } = "1.0";
    //    public string CorrelationId { get; set; } = Guid.NewGuid().ToString("N");
    //    public object Payload { get; set; } = default!;
    //    public string Source { get; set; } = "unknown";
    //    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    //    public IDictionary<string, string> Headers { get; set; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    //}

    public sealed class KafkaMessageEnvelope
    {
        public string Key { get; init; } = "";
        public object Payload { get; init; } = new { };
        public string Source { get; init; } = "MQTT";
        public DateTime Timestamp { get; init; } = DateTime.UtcNow;
        public IDictionary<string, string>? Headers { get; init; }
    }
}
