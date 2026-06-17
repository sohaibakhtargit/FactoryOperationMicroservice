namespace FactoryOperation_NotificationService.FactoryOpsApp.Application.Models
{
    public class ExternalPayloadKafkaMessage
    {
        public string Type { get; set; } = default!;
        public string BlobUrl { get; set; } = default!;
        public long SizeBytes { get; set; }
        public int ChunkCount { get; set; }
        public string? CorrelationId { get; set; }
        public int? TenantId { get; set; }
    }

}
