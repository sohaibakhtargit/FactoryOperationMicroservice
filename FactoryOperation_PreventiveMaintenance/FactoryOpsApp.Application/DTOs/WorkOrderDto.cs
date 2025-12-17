using FactoryOpsApp.Domain.Entities.FactoryOpsTenants;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static FactoryOpsApp.Application.DTOs.WorkOrderCreateDto;

namespace FactoryOpsApp.Application.DTOs
{
    public class WorkOrderDto
    {
        public int WorkOrderId { get; set; }
        public string? WorkOrderNumber { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int? AssetId { get; set; }
        public string? AssetName { get; set; }
        public int? LocationId { get; set; }
        public string? LocationName { get; set; }
        public WorkOrderStatus? Status { get; set; } = WorkOrderStatus.Started;
        public PriorityLevel? Priority { get; set; } = PriorityLevel.Medium;
        public WorkOrderTypeEnum? WorkOrderType { get; set; }
        public int? AssignedToUserId { get; set; }
        public string? AssignedToUser { get; set; }
        public int? AssignedToTeamId { get; set; }
        public string? AssignedToTeam { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime? ScheduleDate { get; set; }
        public int? EstimatedDurationMinutes { get; set; }
        public int? RequiredToolId { get; set; }
        public string? Instructions { get; set; }
        public string? CompletionNotes { get; set; }
        public decimal? LaborCost { get; set; }
        public decimal? PartCost { get; set; }
        public decimal? TotalCost { get; set; }
        public List<WorkOrderToolDto>? RequiredTools
        {
            get; set;

        }

    }
    public class WorkOrderCreateDto
    {
        public int WorkOrderId { get; set; }
        public int TenantId { get; set; }
        public int? AssetId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int? LocationId { get; set; }
        //public IFormFile? WorkOrderPhoto { get; set; }
        //public string? WorkOrderPhotoPath { get; set; }
        public WorkOrderStatus? Status { get; set; } = WorkOrderStatus.Started;
        public PriorityLevel? Priority { get; set; } = PriorityLevel.Medium;
        public WorkOrderTypeEnum? WorkOrderType { get; set; } = WorkOrderTypeEnum.Corrective;
        public int? AssignedToUserId { get; set; }
        public int? AssignedToTeamId { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime? ScheduleDate { get; set; }
        public int? EstimatedDurationMinutes { get; set; } = 60;
        public string? Instructions { get; set; }
        public decimal? LaborCost { get; set; } = 0;
        public decimal? PartCost { get; set; } = 0;
        public int? CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; } = DateTime.UtcNow;
        public int? UpdatedBy { get; set; }
        public List<WorkOrderToolDto>? RequiredTools
        {
            get; set;

        }
        public class WorkOrderToolDto
        {
            public int? ToolId { get; set; }
            public string? ToolName { get; set; } 

            public int? QuantityRequired { get; set; }
        }
        public class WorkOrderUpdateDto : WorkOrderCreateDto
        {
            public int WorkOrderId { get; set; }
            public string? CompletionNotes { get; set; }
        }
        public class WorkOrderProgresssUpdateDto : WorkOrderUpdateDto
        {
            public UpdateTypeEnum? UpdateType { get; set; }
            public int? ProgressPercentage { get; set; }
            public string? LastUpdateMessage { get; set; }
            public IFormFile? WorkOrderPhoto { get; set; }
            public string? WorkOrderPhotoPath { get; set; }
            public string? Comments { get; set; }
            public DateTime? StartTime { get; set; }
            public DateTime? PauseTime { get; set; }
            public bool IsStartd { get; set; } = false;
            public bool IsPaused { get; set; } = false;
            public Decimal TotalTime { get; set; } = 0;
            public string? WorkProgressStatus { get; set; }
            public string? Action { get; set; }

        }

        public class WorkOrderEventDto
        {
            public int WorkOrderId { get; set; }
            public string EventType { get; set; } // Created, Updated, Deleted
            public int TenantId { get; set; }
            public string WorkOrderNumber { get; set; }
            public string Title { get; set; }
            public DateTime EventTime { get; set; } = DateTime.UtcNow;
        }

    }
}
