using System;

namespace FactoryOperation_NotificationService.FactoryOpsApp.Common.Models
{
    public class NotificationModel
    {
        public int TenantId { get; set; }
        public int WorkOrderId { get; set; }
        public string EventType { get; set; } = string.Empty;
        public int? TargetUserId { get; set; }
        public List<int?> TargetUserIds { get; set; } = new();
        public int?Quantity { get; set; }
        public Decimal? Cost { get; set; }

        public int? TargetTeamId { get; set; }
        public string Message { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public int? CreatedByUserId { get; set; }
        public string? AdditionalDataJson { get; set; }
    }
}
