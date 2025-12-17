using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FactoryOpsApp.Domain.Entities.FactoryOpsTenants;
using System;
using System.ComponentModel.DataAnnotations;

namespace FactoryOpsApp.Application.DTOs
{
    public class ServiceRequestDto
    {
        public int ServiceRequestId { get; set; }

        [Required]
        public int TenantId { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

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

        public string Instructions { get; set; } = string.Empty;
        public string CompletionNotes { get; set; } = string.Empty;

        public decimal LaborCost { get; set; } = 0;
        public decimal PartCost { get; set; } = 0;
        public decimal TotalCost { get; set; } = 0;

        public int? CreatedBy { get; set; }
        public int? UpdatedBy { get; set; }
    }

    public class GetServiceRequestDto
    {
        public int ServiceRequestId { get; set; }
        public int TenantId { get; set; }
        public string RequestNumber { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int? LocationId { get; set; }
        public string LocationName { get; set; } = string.Empty;
        public ServiceRequestStatus Status { get; set; }
        public string StatusDisplay { get; set; } = string.Empty;
        public ServiceRequestPriority Priority { get; set; }
        public string PriorityDisplay { get; set; } = string.Empty;
        public int? AssignedToUserId { get; set; }
        public string AssignedUserName { get; set; } = string.Empty;
        public int? AssignedToTeamId { get; set; }
        public string AssignedTeamName { get; set; } = string.Empty;
        public DateTime? DueDate { get; set; }
        public DateTime? ScheduleDate { get; set; }
        public int? EstimatedDurationMinutes { get; set; }
        public ServiceRequestType RequestType { get; set; }
        public string RequestTypeDisplay { get; set; } = string.Empty;
        public string Instructions { get; set; } = string.Empty;
        public string CompletionNotes { get; set; } = string.Empty;
        public decimal LaborCost { get; set; }
        public decimal PartCost { get; set; }
        public decimal TotalCost { get; set; }
        public bool IsActive { get; set; }
        public string DueDateFormatted { get; set; } = string.Empty;
        public string ScheduleDateFormatted { get; set; } = string.Empty;
        public string DaysUntilDue { get; set; } = string.Empty;
    }

    public class ServiceRequestQueryDto
    {
        public int TenantId { get; set; }
        public ServiceRequestStatus? Status { get; set; }
        public ServiceRequestPriority? Priority { get; set; }
        public ServiceRequestType? RequestType { get; set; }
        public int? AssignedToUserId { get; set; }
        public int? LocationId { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }

    public class ServiceRequestStatusUpdateDto
    {
        public int ServiceRequestId { get; set; }
        public int TenantId { get; set; }
        public ServiceRequestStatus Status { get; set; }
        public string? CompletionNotes { get; set; }
        public int UpdatedBy { get; set; }
    }
}