namespace FactoryOperation_IOTDevices.FactoryOpsApp.Application.Models
{
    public class EventTraceEntry
    {
        public string? CorrelationId { get; set; } = "";
        public int? TenantId { get; set; }
        public string Service { get; set; } = "";
        public string Stage { get; set; } = "";
        public string? Topic { get; set; }
        public int? Partition { get; set; }
        public long? Offset { get; set; }
        public string Status { get; set; } = "SUCCESS";
        public string? Message { get; set; }
        public string? Error { get; set; }
        public bool IsActive { get; set; } = true;
        public bool IsDeleted { get; set; } = false;
    }
}
