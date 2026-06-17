namespace FactoryOperation_IOTDevices.FactoryOpsApp.Application.Models
{
    using System.Text.Json.Serialization;

    public sealed class DomainEvent<T>
    {
        [JsonPropertyName("EventType")]
        public string EventType { get; set; } = default!;

        [JsonPropertyName("TenantId")]
        public int TenantId { get; set; }

        [JsonPropertyName("Source")]
        public string SourceService { get; set; } = default!;

        [JsonPropertyName("CorrelationId")]
        public string CorrelationId { get; set; } = default!;

        [JsonPropertyName("OccurredAt")]
        public DateTime? OccurredAtUtc { get; set; }
        public string IpAddress { get; set; } = default!;
        [JsonPropertyName("Payload")]
        public T Payload { get; set; } = default!;
    }

    public class AuditEnvelope
    {
        public string EventType { get; set; } = default!;
        public int? TenantId { get; set; }
        public string SourceService { get; set; } = default!;
        public string CorrelationId { get; set; } = default!;
        public DateTime OccurredAtUtc { get; set; }
    }


}