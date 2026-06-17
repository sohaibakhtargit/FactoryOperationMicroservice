using FactoryOpsApp.Domain.Entities.FactoryOpsTenants;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace FactoryOpsApp.Application.DTOs
{
  public class AlertNotificationDTO
    {
        public int AlertId { get; set; }
        public int TenantId { get; set; }
        public string AlertTitle { get; set; } = string.Empty;
        public string AlertMessage { get; set; } = string.Empty;
        public AlertType AlertType { get; set; }
        public SeverityLevel Severity { get; set; } = SeverityLevel.Medium;
        public string Recipients { get; set; } = string.Empty;

        //public int? RelatedWorkOrderId { get; set; }
        //public WorkOrder? RelatedWorkOrder { get; set; }

        //public int? RelatedTaskId { get; set; }
        //public MaintenanceTask? RelatedTask { get; set; }

        //public int? RelatedAssetId { get; set; }
        //public AssetRegistry? RelatedAsset { get; set; }

        //public DateTime? DueDate { get; set; }
        //public int? DaysBeforeDue { get; set; }

        //public bool IsRead { get; set; } = false;
        //public bool IsAcknowledged { get; set; } = false;
        //public DateTime? AcknowledgedAt { get; set; }
        //public int? AcknowledgedByUserId { get; set; }
        //public FactoryUsers? AcknowledgedByUser { get; set; }

        //public DateTime? SentAt { get; set; }
        //public bool IsSent { get; set; } = false;


        public bool IsActive { get; set; } = true;
        public bool IsDeleted { get; set; } = false;
        public int? CreatedBy { get; set; }
        public int? UpdatedBy { get; set; }
        public int? DeletedBy { get; set; }
        public DateTime? DeletedAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    }
}
