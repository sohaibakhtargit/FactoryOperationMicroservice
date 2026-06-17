namespace FactoryOperation_NotificationService.FactoryOpsApp.Common.Models
{
    public class ChunkedPayloadReference
    {
        public string Type { get; set; }
        public string Key { get; set; }
        public int ChunkCount { get; set; }
        public List<string> BlobUris { get; set; }
    }
}
