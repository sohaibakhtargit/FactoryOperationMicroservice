using System.Text.Json;

namespace FactoryOperation_NotificationService.FactoryOpsApp.Common.Models
{
    public sealed class WorkOrderEventDto : LowStockEventDto
    {
        public int WorkOrderId { get; set; }
        public int TenantId { get; set; }
        public string WorkOrderNumber { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string EventType { get; set; } = string.Empty;
        public DateTime EventTime { get; set; }
        public string? Priority { get; set; }
        public string? Status { get; set; }
        public int? AssignedToUserId { get; set; }
        public int? AssignedToTeamId { get; set; }
        public List<int?> SupervisorUserIds { get; set; } = new();
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
        public List<int?> SupervisorUserIds { get; set; } = new();
    }
    public class LowStockEventDto 
    {
        public int TenantId { get; set; }
        public int ItemId { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public int QuantityAvailable { get; set; }
        public int ReorderLevel { get; set; }

        public string EventType { get; set; } = "LowStock";
        public DateTime EventTime { get; set; } = DateTime.UtcNow;

        /*public int? TargetUserId { get; set; }*/
        public List<int?> TargetUserIds { get; set; } = new();

        public int? TargetTeamId { get; set; }
        
    }

    public class WorkOrderProgressUpdatedEventDto
    {
        public int WorkOrderId { get; set; }
        public string WorkOrderNumber { get; set; } = string.Empty;
        public int TenantId { get; set; }
        public int UpdatedByUserId { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int ProgressPercentage { get; set; }
        public string? Remarks { get; set; }
        public string NewStatus { get; set; }
        public int UpdatedBy { get; set; }
        public string Action { get; set; }
        public List<int?> TargetUserIds { get; set; } = new();

    }

    public class KafkaMessageEnvelope
    {
        public string? Key { get; set; }
        public string? Version { get; set; }
        public string? CorrelationId { get; set; }
        public JsonElement Payload { get; set; }
        public string? Source { get; set; }
        public DateTime Timestamp { get; set; }
        public Dictionary<string, string>? Headers { get; set; }
    }
    public sealed class InventoryEventDto
    {
        public int? PurchaseRequisitionId { get; set; }
        public int TenantId { get; set; }
        public int? InventoryId { get; set; }
        public string InventoryName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public string EventType { get; set; } = string.Empty;
        public DateTime EventTime { get; set; }
        public string? Priority { get; set; }
        public string? Status { get; set; }
        public List<int?> SupervisorUserIds { get; set; } = new();
        public List<int?> TargetUserIds { get; set; } = new();
        public int? SupplierId { get; set; }
        public int? TargetUserId { get; set; }
        public Decimal? Cost { get; set; }

    }

}
