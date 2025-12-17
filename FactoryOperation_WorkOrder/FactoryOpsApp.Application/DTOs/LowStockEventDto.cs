namespace FactoryOperation_WorkOrder.FactoryOpsApp.Application.DTOs
{
    public class LowStockEventDto
    {
        public int TenantId { get; set; }
        public int ItemId { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public int QuantityAvailable { get; set; }
        public int ReorderLevel { get; set; }

        public string EventType { get; set; } = "LowStock";
        public DateTime EventTime { get; set; } = DateTime.UtcNow;
        public List<int?> TargetUserIds { get; set; } = new();

        public int? TargetTeamId { get; set; }
    }
    public class WorkOrderProgressUpdatedEventDto
    {
        public int TenantId { get; set; }
        public int WorkOrderId { get; set; }
        public string WorkOrderNumber { get; set; } = string.Empty;
        public DateTime UpdatedAt { get; set; }
        public Decimal? ProgressPercentage { get; set; }
        public string? Remarks { get; set; }
        public WorkOrderStatus? Status { get; set; }
        public string EventType { get; set; } 
        public string? Action { get; set; }
        public string NewStatus { get; set; }
        public int? UpdatedBy { get; set; }
        public List<int?> TargetUserIds { get; set; }

    }
    public class WorkOrderAssignedEventDto
    {
        public int TenantId { get; set; }
        public int WorkOrderId { get; set; }
        public string WorkOrderNumber { get; set; }
        public int? AssignedToUserId { get; set; }
        public string Title { get; set; }
        public string EventType { get; set; } = "Assigned";
        public DateTime EventTime { get; set; }
        public int? TargetUserId { get; set; }
    }
}
