using FactoryOpsApp.Domain.Entities.FactoryOpsTenants;
using System;

namespace FactoryOperation_NotificationService.FactoryOpsApp.Common.Models
{
    public class NotificationModel
    {
        public int TenantId { get; set; }
        public int? WorkOrderId { get; set; }
        public int? ServiceRequestId {  get; set; }
        public int? outgoingNotifications { get; set; }
        public List<int?> incomingNotifications { get; set; } = new();
        public ServiceRequestType? ServiceRequestType { get; set; }
        public string EventType { get; set; } = string.Empty;
        public string NotificationType { get; set; } = string.Empty;
        public int? TargetUserId { get; set; }
        public List<int?> TargetUsersIds { get; set; } = new();
        public List<string> TargetUsersEmails { get; set; } = new();
        public int? Quantity { get; set; }
        public Decimal? Cost { get; set; }
        public string? LocationName { get; set; } = string.Empty;
        public int? TargetTeamId { get; set; }
        public string Message { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public int? CreatedByUserId { get; set; }
        public int? UpdatedBy { get; set; }
        public string? AdditionalDataJson { get; set; }
        public string? WorkOrderTypeName { get; set; }
        public string? WorkOrderNumber { get; set; }
        public string? ServiceRequestNumber { get; set;}
        public string? ItemCode { get; set; } 
    }
}
