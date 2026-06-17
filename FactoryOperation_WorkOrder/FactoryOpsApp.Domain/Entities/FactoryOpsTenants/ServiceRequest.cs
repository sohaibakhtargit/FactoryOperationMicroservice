using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace FactoryOpsApp.Domain.Entities.FactoryOpsTenants
{
    public class ServiceRequest
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ServiceRequestId { get; set; }

        public int TenantId { get; set; }

        [Required]
        [MaxLength(20)]
        public string RequestNumber { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public int? LocationId { get; set; }

        [Required]
        public ServiceRequestStatus Status { get; set; } = ServiceRequestStatus.Pending;

        [Required]
        public ServiceRequestPriority Priority { get; set; } = ServiceRequestPriority.Medium;

        public int? AssetId { get; set; }

        public int? WorkOrderId { get; set; }

        public int? ApprovedBy { get; set; }
        public DateTime? ApprovedAt { get; set; }

        public int? RejectedBy { get; set; }
        public DateTime? RejectedAt { get; set; }
        public string? ServiceRequestMedia { get; set; }
        public string? ServiceRequestMediaPath { get; set; }
        public string? FileType { get; set; }
        public string? RejectionReason { get; set; }

        public int? AssignedToUserId { get; set; }
        public int? AssignedToTeamId { get; set; }

        public DateTime? DueDate { get; set; }
        public DateTime? ScheduleDate { get; set; }
        public int? EstimatedDurationMinutes { get; set; }

        [Required]
        public ServiceRequestType RequestType { get; set; } = ServiceRequestType.Maintenance;

        public string? Instructions { get; set; }
        public string? CompletionNotes { get; set; }

        public decimal LaborCost { get; set; } = 0;
        public decimal PartCost { get; set; } = 0;
        public decimal TotalCost { get; set; } = 0;

        public bool IsActive { get; set; } = true;
        public bool IsDeleted { get; set; } = false;

        public int? CreatedBy { get; set; }
        public int? UpdatedBy { get; set; }
        public int? DeletedBy { get; set; }

        public DateTime? DeletedAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public ICollection<WorkOrderRequiredTool>? RequiredTools { get; set; }

        [ForeignKey(nameof(AssignedToUserId))]
        public virtual FactoryUsers? AssignedUser { get; set; }

        [ForeignKey(nameof(AssignedToTeamId))]
        public virtual FactoryTeam? AssignedTeam { get; set; }

        [ForeignKey(nameof(LocationId))]
        public virtual Location? Location { get; set; }

        [ForeignKey(nameof(AssetId))]
        public virtual AssetRegistry? Asset { get; set; }

        [ForeignKey(nameof(WorkOrderId))]
        public virtual WorkOrder? WorkOrder { get; set; }

        [ForeignKey(nameof(ApprovedBy))]
        public virtual FactoryUsers? ApprovedUser { get; set; }

        [ForeignKey(nameof(RejectedBy))]
        public virtual FactoryUsers? RejectedUser { get; set; }
    }
}


public enum ServiceRequestStatus
    {
        Pending,
        Approved,
        Rejected,
        Assigned,
        ReOpened,
        InProgress,
        OnHold,
        Completed,
        Closed,
    }

public enum ServiceRequestCategory
    {
        Electrical,
        Mechanical,
        Plumbing,
        HVAC,
        IT,
        GeneralMaintenance,
        Safety
    }


public enum ServiceRequestPriority
    {
        Low,
        Medium,
        High,
        Critical
    }


public enum ServiceRequestType
    {
        Preventive,
        Corrective,
        Maintenance,
        Repair,
        Inspection,
        Cleaning,
        Other
    }
