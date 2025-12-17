using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace FactoryOpsApp.Domain.Entities.FactoryOpsTenants
{
    public class MaintenanceTask
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int TaskId { get; set; }
        public int TenantId { get; set; }

        [Required]
        [MaxLength(200)]
        public string TaskName { get; set; } = string.Empty;

        public int WorkOrderId { get; set; }
        [ForeignKey("WorkOrderId")]
        public WorkOrder WorkOrder { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        [MaxLength(1000)]
        public string? Instructions { get; set; }

        public int SequenceOrder { get; set; } = 1;
        public int EstimatedTimeMinutes { get; set; } = 30;

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public MaintenanceTaskStatus Status { get; set; } = MaintenanceTaskStatus.NotStarted;

        public bool IsMandatory { get; set; } = false;
        public bool VerificationRequired { get; set; } = false;

        [MaxLength(500)]
        public string? Notes { get; set; }

        public DateTime? StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public int? ActualTimeMinutes { get; set; }

        public int? VerifiedByUserId { get; set; }
        [ForeignKey("VerifiedByUserId")]
        public FactoryUsers? VerifiedByUser { get; set; }

        public DateTime? VerifiedAt { get; set; }

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

    public enum MaintenanceTaskStatus
    {
        NotStarted,
        InProgress,
        Completed,
        Skipped,
        Failed
    }
}