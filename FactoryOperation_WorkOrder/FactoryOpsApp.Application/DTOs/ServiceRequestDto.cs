using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FactoryOpsApp.Domain.Entities.FactoryOpsTenants;
using System;
using System.ComponentModel.DataAnnotations;
using static FactoryOpsApp.Application.DTOs.WorkOrderCreateDto;
using FactoryOpsApp.Domain.Entities;

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
        public int? AssetId { get; set; }

        [Required]
        public ServiceRequestStatus? Status { get; set; } = ServiceRequestStatus.Pending;

        [Required]
        public ServiceRequestPriority? Priority { get; set; } = ServiceRequestPriority.Medium;

        public int? AssignedToUserId { get; set; }
        public int? AssignedToTeamId { get; set; }

        public DateTime? DueDate { get; set; }
        public DateTime? ScheduleDate { get; set; }
        public int? EstimatedDurationMinutes { get; set; }
/*        public IFormFile? ServiceRequestImageName { get; set; }
        public string? ServiceRequestImageUrl { get; set; }*/

        [Required]
        public ServiceRequestType? RequestType { get; set; } = ServiceRequestType.Maintenance;

        public string Instructions { get; set; } = string.Empty;
        public string CompletionNotes { get; set; } = string.Empty;

        public decimal? LaborCost { get; set; } = 0;
        public decimal? PartCost { get; set; } = 0;
        public decimal? TotalCost { get; set; } = 0;

        public int? CreatedBy { get; set; }
        public int? UpdatedBy { get; set; }
        // public ICollection<WorkOrderRequiredTool>? RequiredTools { get; set; }
        public List<RequiredToolDto>? RequiredTools { get; set; }
    }
    public class RequiredToolDto
    {
        public int? ToolId { get; set; }
        public string? ToolName { get; set; }
        public int? QuantityRequired { get; set; }
    }
    public class ServiceRequestMediaDto
    {
        public int ServiceRequestId { get; set; }
        public int TenantId { get; set; }

        public IFormFile? ServiceRequestMedia { get; set; }

        public int? UpdatedBy { get; set; }
    }

    public class ApproveServiceRequestDto
    {
        public int TenantId { get; set; }
        public int ServiceRequestId { get; set; }
        public int ApprovedBy { get; set; }
    }

    public class RejectServiceRequestDto
    {
        public int TenantId { get; set; }
        public int ServiceRequestId { get; set; }
        public int RejectedBy { get; set; }
        public string? Reason { get; set; }
    }

    public class AssignServiceRequestDto
    {
        public int TenantId { get; set; }
        public int ServiceRequestId { get; set; }
        public int AssignedToUserId { get; set; }
        public int? AssignedToTeamId { get; set; }
        public int AssignedBy { get; set; }
    }

    public sealed class ServiceRequestEventDto
    {
        public int? WorkOrderId { get; set; }
        public int ServiceRequestId { get; set; }
        public int? outgoingNotifications { get; set; }
        public List<int?> incomingNotifications { get; set; } = new();
        public int ServiceStatus { get; set; }
        public  string ServiceRequestNumber { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int? AssetId { get; set; }
        public int? LocationId { get; set; }
        public decimal LaborCost { get; set; }
        public decimal PartCost { get; set; }
        public decimal TotalCost { get; set; }
        public int TenantId { get; set; }
        public string? TenantEmail { get; set; }

        public WorkOrderTypeEnum? WorkOrderType { get; set; }
        public ServiceRequestType ServiceRequestType { get; set; }
        public string? WorkOrderTypeName { get; set; }
        public string WorkOrderNumber { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string EventType { get; set; } = string.Empty;
        public string NotificationType {  get; set; } = string.Empty;
        public DateTime EventTime { get; set; }
        public string? Priority { get; set; }
        public string? Status { get; set; }
        public int? RejectedBy { get; set; }

        public string? AssignedTo { get; set; }
        public int? AssignedToUserId { get; set; }
        public int? AssignedToTeamId { get; set; }
        public int? DeletedBy { get; set; }
        public List<int?> TargetUserIds { get; set; }= new ();
        public List<int?> SupervisorUserIds { get; set; } = new();
        public List<string> TargetEmailAddresses { get; set; } = new();
        public int? TargetUserId { get; set; }
        public int? CreatedBy { get; set; }
        public string? Reason { get; set; }

        public int? ApprovedBy { get; set; }
        public int? UpdatedBy { get; set; }
    }
    public class GetServiceRequestDto
    {
        public int ServiceRequestId { get; set; }
        public int? WorkOrderId { get; set; }

        public List<RequiredToolDto>? RequiredTools { get; set; }

        public int TenantId { get; set; }
        public int? AssetId { get; set; }
        public string? AssetName { get; set; }

        public string RequestNumber { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int? LocationId { get; set; }
        public string LocationName { get; set; } = string.Empty;
        public string? ServiceRequestMedia { get; set; }
        public string? ServiceRequestMediaPath { get; set; }
        public string? FileType { get; set; }
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
        public int? CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
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