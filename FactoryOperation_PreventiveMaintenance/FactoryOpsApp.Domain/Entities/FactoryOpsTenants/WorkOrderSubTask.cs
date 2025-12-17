using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace FactoryOpsApp.Domain.Entities.FactoryOpsTenants
{
    [Table("WorkOrderSubTasks")]
    public class WorkOrderSubTask
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int SubTaskId { get; set; }
        [Required]
        public int WorkOrderId { get; set; }
        public int? ParentTaskId { get; set; }
        [Required, MaxLength(200)]
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        [MaxLength(20)]
        public PriorityLevel Priority { get; set; } = PriorityLevel.Medium;
        public SubTaskStatus Status { get; set; } = SubTaskStatus.Started;
        [Required]
        public int EstimatedMinutes { get; set; }
        public int? ActualMinutes { get; set; }
        public int? AssignedToUserId { get; set; }
        public int? AssignedToTeamId { get; set; }
        public int Sequence { get; set; } = 0;
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime? CompletedDate { get; set; }
        public int TenantId { get; set; }
        public bool IsActive { get; set; } = true;
        public int? CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // =========================================================================
        // MIGRATED FIELDS FROM MaintenanceTask (NEW - NULLABLE)
       
        [MaxLength(1000)]
        public string? Instructions { get; set; }
        public bool? IsMandatory { get; set; }
        public bool? VerificationRequired { get; set; }
        [MaxLength(500)]
        public string? Notes { get; set; }
        public int? VerifiedByUserId { get; set; }
        public DateTime? VerifiedAt { get; set; }
        public bool IsDeleted { get; set; }
        public int? UpdatedBy { get; set; }
        public int? DeletedBy { get; set; }
        public DateTime? DeletedAt { get; set; }

        // END OF MIGRATED FIELDS
        
        [ForeignKey("WorkOrderId")]
        public virtual WorkOrder WorkOrder { get; set; }

        [ForeignKey("ParentTaskId")]
        public virtual WorkOrderSubTask ParentTask { get; set; }

        [ForeignKey("AssignedToUserId")]
        public virtual FactoryUsers AssignedToUser { get; set; }

        [ForeignKey("AssignedToTeamId")]
        public virtual FactoryTeam AssignedToTeam { get; set; }

        [ForeignKey("VerifiedByUserId")]
        public virtual FactoryUsers? VerifiedByUser { get; set; }

        public virtual ICollection<WorkOrderSubTask> ChildTasks { get; set; } = new List<WorkOrderSubTask>();
    }

    public enum SubTaskStatus
    {
        NotStarted=1,
        Started=2,
        InProgress=3,
        Completed=4,
        Blocked=5,
        Skipped=6,
        Failed=7
    }
}