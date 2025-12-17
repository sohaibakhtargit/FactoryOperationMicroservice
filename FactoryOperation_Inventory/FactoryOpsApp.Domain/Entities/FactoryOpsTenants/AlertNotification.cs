using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace FactoryOpsApp.Domain.Entities.FactoryOpsTenants
{
    public class AlertNotification
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int AlertId { get; set; }
        public int TenantId { get; set; }

        [Required]
        [MaxLength(100)]
        public string AlertTitle { get; set; } = string.Empty;

        [MaxLength(500)]
        public string AlertMessage { get; set; } = string.Empty;

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public AlertType AlertType { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public SeverityLevel Severity { get; set; } = SeverityLevel.Medium;

        [MaxLength(500)]
        public string Recipients { get; set; } = string.Empty; 

        public int? RelatedWorkOrderId { get; set; }
        [ForeignKey("RelatedWorkOrderId")]
        public WorkOrder? RelatedWorkOrder { get; set; }

        public int? RelatedTaskId { get; set; }
        [ForeignKey("RelatedTaskId")]
        public MaintenanceTask? RelatedTask { get; set; }

        public int? RelatedAssetId { get; set; }
        [ForeignKey("RelatedAssetId")]
        public AssetRegistry? RelatedAsset { get; set; }

        public DateTime? DueDate { get; set; }
        public int? DaysBeforeDue { get; set; }

        public bool IsRead { get; set; } = false;
        public bool IsAcknowledged { get; set; } = false;
        public DateTime? AcknowledgedAt { get; set; }
        public int? AcknowledgedByUserId { get; set; }
        [ForeignKey("AcknowledgedByUserId")]
        public FactoryUsers? AcknowledgedByUser { get; set; }

        public DateTime? SentAt { get; set; }
        public bool IsSent { get; set; } = false;

        // Audit Fields
        public bool IsActive { get; set; } = true;
        public bool IsDeleted { get; set; } = false;
        public int? CreatedBy { get; set; }
        public int? UpdatedBy { get; set; }
        public int? DeletedBy { get; set; }
        public DateTime? DeletedAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }

    public enum AlertType
    {
        UpcomingTask,
        OverdueTask,
        MaintenanceDue,
        NewWorkOrder,
        TaskCompleted,
        WorkOrderCompleted,
        AssetIssue
    }

    public enum SeverityLevel
    {
        Low,
        Medium,
        High,
        Critical
    }
}