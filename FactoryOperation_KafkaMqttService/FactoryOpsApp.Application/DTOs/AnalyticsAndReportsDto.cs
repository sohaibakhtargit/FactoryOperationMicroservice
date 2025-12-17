using FactoryOpsApp.Domain.Entities.FactoryOpsTenants;

namespace FactoryOpsApp.Application.DTOs
{
    public class AnalyticsAndReportsDto
    {
        public CategoryType Category { get; set; }
        public decimal? TotalCost { get; set; }
        public double? ComplianceRate { get; set; }
        public double? TotalDowntimeHours { get; set; }
        public double? MTBF { get; set; } 
        public int WorkOrdersCount { get; set; }
        public int MaintenanceTasksCount { get; set; }
        public int MaintenanceSchedulesCount { get; set; }
        public int MaintenanceSchedules {get; set; }
        public MetricsDistributionsDto MetricsDistribution { get; set; } = new MetricsDistributionsDto();
    }
    public class MetricsDistributionsDto
    {
        public decimal? CostPercentage { get; set; } = 0;      
        public decimal? CompliancePercentage { get; set; } = 0;
        public decimal? DowntimePercentage { get; set; } = 0;   
        public decimal? ReliabilityPercentage { get; set; } = 0;  
    }

    public enum CategoryType
    {
        All,
        Cost,
        Compliance,
        Downtime,
        Reliability
    }
}
