using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FactoryOpsApp.Application.DTOs
{
    public class LaborResourceAnalyticsDto
    {
        public int TotalTechnicians { get; set; }
        public int AvailableTechnicians { get; set; }
        public double AvgUtilization { get; set; }
        public double AvgUtilizationChange { get; set; }
        public int ActiveWorkOrders { get; set; }
        public double ResourceEfficiency { get; set; }
        public double ResourceUsage { get; set; }
        public WorkloadDistributionDto WorkloadDistribution { get; set; }
        public PerformanceMetricsDto PerformanceMetrics { get; set; }
        public CostAnalysisDto CostAnalysis { get; set; }
        public LaborCostBreakdownDto LaborCostBreakdown { get; set; }
    }

    public class WorkloadDistributionDto
    {
        public int Balanced { get; set; }
        public int Overloaded { get; set; }
        public int Underutilized { get; set; }
    }

    public class PerformanceMetricsDto
    {
        public double AvgResponseTime { get; set; }
        public double FirstTimeFixRate { get; set; }
        public double TeamEfficiency { get; set; }
    }

    public class CostAnalysisDto
    {
        public decimal RegularHours { get; set; }
        public decimal Overtime { get; set; }
        public decimal ResourceCosts { get; set; }
        public decimal TotalCost { get; set; }
    }

    public class LaborCostBreakdownDto
    {
        public decimal RegularHours { get; set; }
        public decimal Overtime { get; set; }
        public decimal ResourceCosts { get; set; }
        public decimal TotalCost { get; set; }
    }
}
