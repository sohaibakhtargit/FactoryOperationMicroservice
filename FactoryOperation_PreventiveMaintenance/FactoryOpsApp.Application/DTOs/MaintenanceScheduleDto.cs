using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FactoryOpsApp.Domain.Entities.FactoryOpsTenants;
using System;
using System.ComponentModel.DataAnnotations;
using FactoryOpsApp.Domain.Entities;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http;

namespace FactoryOpsApp.Application.DTOs
{
    public class MaintenanceScheduleDto
    {
        public int? ScheduleId { get; set; }

        [Required]
        public int TenantId { get; set; }
        public int? LocationId { get; set; }

        [Required]
        [MaxLength(150)]
        public string ScheduleName { get; set; } = string.Empty;

        [Required]
        public ScheduleType ScheduleType { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        [Required]
        public FrequencyType Frequency { get; set; }

        [Required]
        public int FrequencyValue { get; set; } = 1;

        [Required]
        public DateTime StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        [MaxLength(200)]
        public string? LocationFilter { get; set; }
        public int? PrimarySubTaskId { get; set; }      
        public List<int>? SubTaskIds { get; set; }
        public WorkOrderStatus Status { get; set; } = WorkOrderStatus.Active;
        public DateTime? NextDueDate { get; set; }
        public int? CreatedBy { get; set; }
        public int? UpdatedBy { get; set; }
        public int? AssetId { get; set; }
        public string? ImageUrl { get; set; }
        public IFormFile? Image { get; set; }
        public int? WorkOrderId { get; set; }
        public string? WorkOrderNumber { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public InventoryCategoryEnum? Category { get; set; }
    }
    public class GetMaintenanceScheduleDto
    {
        public int? PrimarySubTaskId { get; set; }
        public List<int>? SubTaskIds { get; set; }
        public int ScheduleId { get; set; }
        public int TenantId { get; set; }
        public string ScheduleName { get; set; } = string.Empty;
        public ScheduleType ScheduleType { get; set; }
        public string? Description { get; set; }
        public FrequencyType Frequency { get; set; }
        public int FrequencyValue { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? LocationFilter { get; set; }
        public WorkOrderStatus Status { get; set; }
        public DateTime? NextDueDate { get; set; }
        public bool IsActive { get; set; }
        public string NextDueFormatted { get; set; } = string.Empty;
        public string FrequencyDisplay { get; set; } = string.Empty;
        public int ActiveWorkOrders { get; set; }
        public int? AssetId { get; set; }
        public string? Asset { get; set; }
        public string? ImageUrl { get; set; }
        public string? Image { get; set; }
        public int? WorkOrderId { get; set; }
        public string? WorkOrderNumber { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public InventoryCategoryEnum? Category { get; set; }
        public List<MaintenanceScheduleOccurrenceDto> Occurrences { get; set; } = new();

    }

    public class ScheduleApprovalDto
    {
        public int ScheduleId { get; set; }
        public int TenantId { get; set; }
        public bool IsApproved { get; set; }
        public string? RejectionReason { get; set; }
        public int ApprovedBy { get; set; }
    }
}