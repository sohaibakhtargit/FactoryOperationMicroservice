using static FactoryOperation_WorkOrder.FactoryOpsApp.Application.DTOs.TechnicianLoadDto;

namespace FactoryOperation_WorkOrder.FactoryOpsApp.Application.DTOs
{
    public class AttachmentDto
    {
        public string? FileName { get; set; }
        public string? FilePath { get; set; }
    }

    public class CalendarEventDto
    {
        public int WorkOrderId { get; set; }
        public string WorkOrderNumber { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string? FileName { get; set; }
        public string? FilePath { get; set; }
        public string? Description { get; set; } 
        public string? Priority { get; set; }
        public string? Status { get; set; }
        public string? WorkOrderType { get; set; }
        public string? AssetName { get; set; }
        public string? LocationName { get; set; }
        public DateTime? Start { get; set; }
        public DateTime? End { get; set; }
        public int? EstimatedDurationMinutes { get; set; }
        public int? AssignedToUserId { get; set; }
        public string? TechnicianName { get; set; }
        public List<AttachmentDto>? Attachments { get; set; }
        public decimal? LaborCost { get; set; }
        public decimal? PartsCost { get; set; }
    }

    public class ConflictDto
    {
        public int WorkOrderId1 { get; set; }
        public int WorkOrderId2 { get; set; }
        public int? TechnicianId { get; set; }
        public string? TechnicianName { get; set; }

        public DateTime? Start1 { get; set; }
        public DateTime? End1 { get; set; }

        public DateTime? Start2 { get; set; }
        public DateTime? End2 { get; set; }
    }

    public class UnscheduledItemDto
    {
        public int WorkOrderId { get; set; }
        public string WorkOrderNumber { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; } 
        public string? Priority { get; set; }
        public string? Status { get; set; }
        public string? WorkOrderType { get; set; }
        public string? LocationName { get; set; }
        public string? Category { get; set; }
        public string? Inventory { get; set; }
        public int? EstimatedDurationMinutes { get; set; }
        public DateTime? DueDate { get; set; }
    }

    public class TechnicianLoadDto
    {
        public int? TechnicianId { get; set; }
        public string? TechnicianName { get; set; }
        public int TotalAssignedMinutes { get; set; }
        public decimal TotalAssignedHours => Math.Round(TotalAssignedMinutes / 60m, 2);

        public class ResourceDto
        {
            public string? AssetName { get; set; }
            public List<ResourceTaskDto> Tasks { get; set; } = new();
        }

        public class ResourceTaskDto
        {
            public int WorkOrderId { get; set; }
            public string WorkOrderNumber { get; set; } = string.Empty;
            public string Title { get; set; } = string.Empty;
            public string? WorkOrderType { get; set; }
            public string? TechnicianName { get; set; }
            public DateTime? Start { get; set; }
            public DateTime? End { get; set; }
        }

        public class WorkOrderCalendarUpdateDto
        {
            public int WorkOrderId { get; set; }
            public int TenantId { get; set; }
            public DateTime? ScheduleDate { get; set; }
            public int? AssignedToUserId { get; set; }
            public int? AssignedToTeamId { get; set; }
            public int? UpdatedBy { get; set; }
        }
    }

    public class CalendarDataDto
    {
        public List<CalendarEventDto> ScheduledEvents { get; set; } = new();
        public List<ConflictDto> Conflicts { get; set; } = new();
        public List<UnscheduledItemDto> UnscheduledItems { get; set; } = new();
        public List<TechnicianLoadDto> TechnicianLoads { get; set; } = new();
        public List<ResourceDto> Resources { get; set; } = new();
    }
}