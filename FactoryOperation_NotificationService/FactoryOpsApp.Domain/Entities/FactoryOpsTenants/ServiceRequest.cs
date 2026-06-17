using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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
        public string RequestNumber { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; }

        public string Description { get; set; }

        public int? LocationId { get; set; }

        [Required]
        public ServiceRequestStatus Status { get; set; } = ServiceRequestStatus.Pending;

        [Required]
        public ServiceRequestPriority Priority { get; set; } = ServiceRequestPriority.Medium;

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

        // Navigation properties with explicit ForeignKey attributes
        [ForeignKey("AssignedToUserId")]
        public virtual FactoryUsers AssignedUser { get; set; }

        [ForeignKey("AssignedToTeamId")]
        public virtual FactoryTeam AssignedTeam { get; set; }

        [ForeignKey("LocationId")]
        public virtual Location Location { get; set; }
    }

    // Enums
    public enum ServiceRequestStatus
    {
        Pending,
        Assigned,
        InProgress,
        Completed,
        Cancelled,
        Overdue,
        Started
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
}
