using FactoryOpsApp.Domain.Entities.FactoryOpsTenants;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FactoryOperation_NotificationService.FactoryOpsApp.Common.Models
{
    public sealed class WorkOrderEventDto : LowStockEventDto
    {
        public int WorkOrderId { get; set; }
        public int NotificationId { get; set; }
        public int? outgoingNotifications { get; set; }
        public List<int?> incomingNotifications { get; set; } = new();
        public int TenantId { get; set; }
        public WorkOrderTypeEnum? WorkOrderType { get; set; }
        public string? WorkOrderTypeName { get; set; }
        public string WorkOrderNumber { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string EventType { get; set; } = string.Empty;
        public DateTime EventTime { get; set; }
        public int? DeletedBy { get; set; }
        public string? Priority { get; set; }
        public string? Status { get; set; }
        public int? AssignedToUserId { get; set; }
        public int? TargetUserId { get; set; }
        public int? AssignedToTeamId { get; set; }
        public List<int?> TargetUsersIds { get; set; } = new();
        public List<string> TargetEmailAddresses { get; set; } = new();   
        public int? CreatedBy { get; set; }
        public string? Message { get; set; }
    }
    public class WorkOrderAssignedEventDto
    {
        public int TenantId { get; set; }
        public int WorkOrderId { get; set; }
        public int NotificationId { get; set; }
        public string WorkOrderNumber { get; set; }
        public string? WorkOrderTypeName { get; set; }
        public int? AssignedToUserId { get; set; }
        public int? outgoingNotifications { get; set; }
        public List<int?> incomingNotifications { get; set; } = new();
        public List<int?> TargetUsersIds { get; set; } = new();
        public List<string> TargetUsersEmails { get; set; } = new();
        public string AssignedTo { get; set; }
        public string Title { get; set; }
        public string EventType { get; set; } = "Assigned";
        public DateTime EventTime { get; set; }
        public int? TargetUserId { get; set; }
        public List<int?> SupervisorUserIds { get; set; } = new();
        public List<string> TargetEmailAddresses { get; set; } = new();
        public string? Status { get; set; }
    }
    public class LowStockEventDto 
    {
        public int TenantId { get; set; }
        public int ItemId { get; set; }
        public int NotificationId { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public int QuantityAvailable { get; set; }
        public int ReorderLevel { get; set; }
        public string ItemCode { get; set; } = string.Empty;
        public string EventType { get; set; } = "LowStock";
        public DateTime EventTime { get; set; } = DateTime.UtcNow;

        /*public int? TargetUserId { get; set; }*/
        public List<int?> TargetUsersIds { get; set; } = new();
        public List<int?> incomingNotifications { get; set; } = new();
        public int? TargetTeamId { get; set; }
        public List<string> TargetEmailAddresses { get; set; } = new();

    }

    public class WorkOrderProgressUpdatedEventDto
    {
        public int WorkOrderId { get; set; }
        public int NotificationId { get; set; }
        public string WorkOrderNumber { get; set; } = string.Empty;
        public string? WorkOrderTypeName { get; set; }
        public string? Title { get; set; }
        public int TenantId { get; set; }
        public int UpdatedByUserId { get; set; }
        public DateTime UpdatedAt { get; set; }
        public decimal? ProgressPercentage { get; set; }
        public string? Remarks { get; set; }
        public string Status { get; set; }
        public string EventType { get; set; } = string.Empty;
        public int UpdatedBy { get; set; }
        public string? TechnicianName { get; set; }
        public string? Action { get; set; }
        public List<int?> TargetUsersIds { get; set; } = new();
        public List<string> TargetEmailAddresses { get; set; } = new();
        public int? outgoingNotifications { get; set; }
        public List<int?> incomingNotifications { get; set; } = new();

    }

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

    public class AssetEventDto
    {
        public int AssetId { get; set; }
        public int NotificationId { get; set; }
        public string AssetName { get; set; } = string.Empty;
        public int TenantId { get; set; }
        public string EventType { get; set; } = string.Empty;
        public int LocationId { get; set; }
        public string? Message { get; set; }
        public string LocationName { get; set; }
        public string LocationType { get; set; }
        public string? EventDescription { get; set; }
        public DateTime EventTime { get; set; } = DateTime.UtcNow;
        public string? Status { get; set; }
        public int? TargetUserId { get; set; }
        public List<int?> SupervisorUserIds { get; set; } = new();
        public List<string> TargetEmailAddresses { get; set; } = new();

    }

    public sealed class InventoryEventDto
    {
        public int? PurchaseRequisitionId { get; set; }
        public int NotificationId { get; set; }
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
        public List<string> TargetEmailAddresses { get; set; } = new();
        public int? SupplierId { get; set; }
        public int? TargetUserId { get; set; }
        public Decimal? Cost { get; set; }

    }
    public sealed class ServiceRequestEventDto
    {
        public int? ServiceRequestId { get; set; }

        public int NotificationId { get; set; }
        public int? outgoingNotifications { get; set; }
        public List<int?> incomingNotifications { get; set; } = new();
        public List<string> TargetUsersEmails { get; set; } = new();
        public int? ServiceStatus { get; set; }
        public int? WorkOrderId { get; set; }
        public int? RejectedBy { get; set; }
        public int? UpdatedBy { get; set; }
        public string Reason { get; set; } = string.Empty;
        public string ServiceRequestNumber { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int? AssetId { get; set; }
        public int? LocationId { get; set; }
        public int? ApprovedBy { get; set; }
        public string? AssignedTo { get; set; }

        public int? CreatedBy { get; set; } 
        public int? TenantId { get; set; }

        public WorkOrderTypeEnum? WorkOrderType { get; set; }

        [JsonPropertyName("ServiceRequestType")]
        public ServiceRequestType? RequestType { get; set; }

        public string? WorkOrderTypeName { get; set; }
        public string WorkOrderNumber { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string EventType { get; set; } = string.Empty;
        public DateTime EventTime { get; set; }
        public string? Priority { get; set; }
        public string? Status { get; set; }
        public int? AssignedToUserId { get; set; }
        public int? AssignedToTeamId { get; set; }
        public int? DeletedBy { get; set; }

        public List<int?> TargetUserIds { get; set; } = new();
        public List<int?> SupervisorUserIds { get; set; } = new();
        public List<string> TargetEmailAddresses { get; set; } = new();
        public int? TargetUserId { get; set; }

        [JsonExtensionData]
        public Dictionary<string, JsonElement>? ExtraFields { get; set; }
    }
}
