namespace FactoryOperation_KafkaMqttService.FactoryOpsApp.Shared.Models
{
    public class ExternalPayloadKafkaMessage
    {
        public string Type { get; set; } = "EXTERNAL_PAYLOAD";
        public string BlobUrl { get; set; } = default!;
        public long SizeBytes { get; set; }
        public int ChunkCount { get; set; }
        public string? CorrelationId { get; set; }
        public int? TenantId { get; set; }
        public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
    }

}
