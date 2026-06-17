using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace FactoryOpsApp.Application.DTOs
{
    public class NotificationDto
    {
        public int NotificationId { get; set; }
        public int TenantId { get; set; }
        public string? Module { get; set; }
        public int? EntityId { get; set; }
        public int? ServiceRequestId { get; set; }
        public string? ServiceRequestNumber { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public string? EventType { get; set; }
        [JsonPropertyName("NotificationType")]
        public string NotificationType { get; set; }
        public string? workOrderTypeName { get; set; }
        public int workOrderId { get; set; }
        public string? WorkOrderNumber { get; set; }
        public string? ItemCode { get; set; }
        public int? TargetUserId { get; set; }
        public int? TargetTeamId { get; set; }
        public int? IncomingNotifications { get; set; }
        public int? OutgoingNotifications { get; set; }
        public bool IsRead { get; set; }
        public bool IsReadByTenant { get; set; }
        public DateTime CreatedAt { get; set; }
        public int? CreatedBy { get; set; }
        public string? AdditionalData { get; set; }
    }
}
