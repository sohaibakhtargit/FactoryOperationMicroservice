using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FactoryOpsApp.Application.DTOs
{
    public class WorkOrderNotificationDto
    {
        public int TenantId { get; set; }
        public int WorkOrderId { get; set; }
        public string WorkOrderNumber { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string EventType { get; set; } = "Assigned";
        public int? AssignedToUserId { get; set; }
        public int? AssignedToTeamId { get; set; }
        public int? CreatedByUserId { get; set; }
        public DateTime EventTime { get; set; } = DateTime.UtcNow;
        public string Priority { get; set; }
        public string Status { get; set; }
    }

    public class InventoryNotificationDto
    {
        public int TenantId { get; set; }
        public int ItemId { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public int QuantityAvailable { get; set; }
        public int ReorderLevel { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public DateTime EventTime { get; set; } = DateTime.UtcNow;
        public int? TargetUserId { get; set; } // manager
        public string EventType { get; set; } = "LowStock";
    }

}
