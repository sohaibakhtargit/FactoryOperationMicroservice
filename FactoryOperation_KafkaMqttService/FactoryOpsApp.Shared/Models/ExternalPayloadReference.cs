namespace FactoryOperation_KafkaMqttService.FactoryOpsApp.Shared.Models
{
    public class ExternalPayloadReference
    {
        public string BlobUrl { get; set; } = default!;
        public long SizeBytes { get; set; }
        public int ChunkCount { get; set; }
    }

}
