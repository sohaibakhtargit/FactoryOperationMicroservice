using System.Text.Json;

namespace FactoryOperation_AuditTrailService.FactoryOpsApp.Application.Models
{
    public class KafkaMessageEnvelope
    {
        public string? Key { get; set; }
        public string? Version { get; set; }
        public string? CorrelationId { get; set; }
        public int TenantId { get; set; }
        public JsonElement Payload { get; set; }
        public string? Source { get; set; }
        public DateTime Timestamp { get; set; }
        public Dictionary<string, string>? Headers { get; set; }
    }
}
