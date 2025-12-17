using System;
using System.Collections.Generic;

namespace FactoryOpsApp.Domain.Entities.FactoryOpsTenants
{
    public class AnalyticsAndReports
    {
        public List<CategoryMetricsDto> CategoryMetrics { get; set; } = new List<CategoryMetricsDto>();
        public List<ComplianceTrendDto> ComplianceTrend { get; set; } = new List<ComplianceTrendDto>();
        public List<CostTrendDto> CostTrend { get; set; } = new List<CostTrendDto>();
        public List<DowntimeTrendDto> DowntimeTrend { get; set; } = new List<DowntimeTrendDto>();
        public MetricsDistributionDto MetricsDistribution { get; set; } = new MetricsDistributionDto();
    }

    public class CategoryMetricsDto
    {
        public decimal Value { get; set; }
        public string DisplayValue { get; set; }
        public string Label { get; set; }
        public DateTime LastUpdated { get; set; }
    }
    public class ComplianceTrendDto
    {
        public DateTime Date { get; set; }
        public decimal ComplianceRate { get; set; }
    }

    public class CostTrendDto
    {
        public DateTime Date { get; set; }
        public decimal Cost { get; set; }
    }

    public class DowntimeTrendDto
    {
        public DateTime Date { get; set; }
        public decimal DowntimeHours { get; set; }
    }

    public class MetricsDistributionDto
    {
        public decimal CostPercentage { get; set; }
        public decimal CompliancePercentage { get; set; }
        public decimal DowntimePercentage { get; set; }
        public decimal ReliabilityPercentage { get; set; }
    }

    public class AnalyticsFilterDto
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? LocationId { get; set; }
        public int? AssetId { get; set; }
        public int? TeamId { get; set; }
    }
}

