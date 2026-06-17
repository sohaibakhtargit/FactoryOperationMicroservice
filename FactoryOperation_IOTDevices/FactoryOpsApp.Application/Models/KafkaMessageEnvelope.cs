using System.Text.Json;

namespace FactoryOperation_IOTDevices.FactoryOpsApp.Application.Models
{
    public class KafkaMessageEnvelope
    {
        
        public string Key { get; init; } = string.Empty;

        public string MessageId { get; init; } = Guid.NewGuid().ToString("N");

        public string? EventType { get; init; } = string.Empty;
        public string? IpAddress { get; init; } = string.Empty;

        public string EventVersion { get; init; } = "v1";
        public string Producer { get; init; } = string.Empty;
        public string Source { get; init; } = "MQTT";
        public int? TenantId { get; init; }

        public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
        public DateTime Timestamp { get; init; } = DateTime.UtcNow;

        public object Payload { get; init; } = new { };

        public IDictionary<string, string>? Headers { get; init; }

        public string? CorrelationId { get; init; }
        public string? CausationId { get; init; }
     
    }
}
