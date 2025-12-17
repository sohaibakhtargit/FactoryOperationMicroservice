using FactoryOpsApp.Domain.Entities.FactoryOpsTenants;
using System;
using System.ComponentModel.DataAnnotations;

namespace FactoryOpsApp.Application.DTOs
{
    public class MaintenanceTaskDto
    {
        public int TaskId { get; set; }

        [Required]
        public int TenantId { get; set; }

        [Required]
        [MaxLength(200)]
        public string TaskName { get; set; } = string.Empty;

        [Required]
        public int WorkOrderId { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        [MaxLength(1000)]
        public string? Instructions { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int SequenceOrder { get; set; } = 1;

        [Range(1, 1440)]
        public int EstimatedTimeMinutes { get; set; } = 30;

        public SubTaskStatus Status { get; set; } = SubTaskStatus.Started;
        public bool IsMandatory { get; set; } = false;
        public bool VerificationRequired { get; set; } = false;

        [MaxLength(500)]
        public string? Notes { get; set; }

        public DateTime? StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public int? ActualTimeMinutes { get; set; }

        public int? VerifiedByUserId { get; set; }
        public DateTime? VerifiedAt { get; set; }

        public int? CreatedBy { get; set; }
        public int? UpdatedBy { get; set; }
    }

    public class GetMaintenanceTaskDto
    {
        public int TaskId { get; set; }
        public int TenantId { get; set; }
        public string TaskName { get; set; } = string.Empty;
        public int WorkOrderId { get; set; }
        public string WorkOrderNumber { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Instructions { get; set; }
        public int SequenceOrder { get; set; }
        public int EstimatedTimeMinutes { get; set; }
        public SubTaskStatus Status { get; set; }
        public bool? IsMandatory { get; set; }
        public bool? VerificationRequired { get; set; }
        public string? Notes { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public int? ActualTimeMinutes { get; set; }
        public int? VerifiedByUserId { get; set; }
        public string? VerifiedByUserName { get; set; }
        public DateTime? VerifiedAt { get; set; }
        public bool IsActive { get; set; }
        public string StatusDisplay { get; set; } = string.Empty;
        public string TimeSpent { get; set; } = string.Empty;
        public string? VerificationStatus { get; set; } = string.Empty;
    }

    public class TaskStatusUpdateDto
    {
        public int TaskId { get; set; }
        public int TenantId { get; set; }
        public SubTaskStatus Status { get; set; }
        public string? Notes { get; set; }
        public int? ActualTimeMinutes { get; set; }
        public int UpdatedBy { get; set; }
    }

    public class TaskVerificationDto
    {
        public int TaskId { get; set; }
        public int TenantId { get; set; }
        public int VerifiedByUserId { get; set; }
        public string? VerificationNotes { get; set; }
    }
}