using System.Text.Json.Serialization;

namespace FactoryOps_AccessManagementService.FactoryOpsApp.Application.DTOs
{

    public class WorkOrderTypeSummaryForTechinicianDto
    {
        public string? Type { get; set; }
        public int Count { get; set; }
        public decimal Percentage { get; set; }
    }

    public class WorkOrderPrioritySummaryForTechinicianDto
    {
        public string? Priority { get; set; }
        public int Count { get; set; }
        public decimal Percentage { get; set; }
    }

    public class TechnicianPerformanceDto
    {
        public int AssignedWorkOrders { get; set; }
        public int CompletedWorkOrders { get; set; }
        public int CancelledWorkOrders { get; set; }
        public int PendingTasks { get; set; }
        public int ActiveWorkOrders { get; set; }
        public int InactiveWorkOrders { get; set; }
        public decimal OnTimeCompletionRate { get; set; }
        public decimal AverageCompletionTimeInHours { get; set; }
        public decimal ReworkRate { get; set; }
    }

    public class TechnicianDashboardDto
    {
        public int TotalActiveWorkOrders { get; set; }
        public int TotalTeams { get; set; }

        public TechnicianPerformanceDto Performance { get; set; } = new();

        public List<WorkOrderTypeSummaryForTechinicianDto> WorkOrdersByType { get; set; } = new();
        public List<WorkOrderPrioritySummaryForTechinicianDto> WorkOrdersByPriority { get; set; } = new();
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum DashboardFilter
    {
        Day,
        Week,
        Month,
        Year
    }
}
