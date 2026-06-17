namespace FactoryOperation_WorkOrder.FactoryOpsApp.Application.DTOs
{
    public class WorkOrderTimelineDto
    {
        public int WorkOrderId { get; set; }
        public string WorkOrderNumber { get; set; }

        public List<WorkOrderTimelineItemDto> Timeline { get; set; } = new();
    }
    public class WorkOrderTimelineItemDto
    {
        public int UpdateId { get; set; }

        public UpdateTypeEnum UpdateType { get; set; }

        public WorkOrderStatus? Status { get; set; }

        public decimal? ProgressPercentage { get; set; }

        public string? Message { get; set; }

        public string? AttachmentPath { get; set; }
        public string? FileType { get; set; }

        public string? Action { get; set; }

        public int? AssignedToUserId { get; set; }

        public DateTime CreatedAt { get; set; }
    }
    public class RecentWorkOrderUpdateDto
    {
        public int WorkOrderId { get; set; }

        public string WorkOrderNumber { get; set; }

        public UpdateTypeEnum UpdateType { get; set; }

        public WorkOrderStatus? Status { get; set; }

        public decimal? ProgressPercentage { get; set; }

        public string? Message { get; set; }

        public string? Action { get; set; }

        public string? AttachmentPath { get; set; }
        public string? FileType { get; set; }

        public int? AssignedToUserId { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
