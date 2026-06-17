using System;
using System.Collections.Generic;

namespace FactoryOpsApp.Application.DTOs
{
    public class AssetDashboardReportDto
    {
        public int DashboardId { get; set; }
        public int TenantId { get; set; }

        public DateTime PeriodDate { get; set; }
        public string PeriodType { get; set; } = "Monthly";

        public decimal OverallUptime { get; set; }
        public decimal UptimeTarget { get; set; }
        public decimal ReliabilityIndex { get; set; }
        public decimal ReliabilityTarget { get; set; }
        public decimal CostEfficiency { get; set; }
        public decimal CostEfficiencyTarget { get; set; }
        public decimal ComplianceRate { get; set; }
        public decimal ComplianceTarget { get; set; }

        public int AssetsCurrentlyDown { get; set; }
        public double AverageDowntimeMinutes { get; set; }
        public double TotalDowntimeHours { get; set; }
        public double DowntimePercentage { get; set; }

        public int RunningAssets { get; set; }
        public int MaintenanceAssets { get; set; }
        public int IdleAssets { get; set; }

        public int Next12MonthsReplacements { get; set; }
        public int TwoToFiveYearsReplacements { get; set; }
        public int FivePlusYearsReplacements { get; set; }

        public string? SelectedReportType { get; set; }
        public string? SelectedDateRangeOption { get; set; }
        public DateTime? CustomStartDate { get; set; }
        public DateTime? CustomEndDate { get; set; }
        public string? SelectedAssetFilter { get; set; }
        public string? SelectedSpecificAssets { get; set; }
        public string? SelectedExportFormat { get; set; }

        public string? ReportName { get; set; }
        public string ReportStatus { get; set; } = "Draft";
        public DateTime? ReportGeneratedAt { get; set; }
        public string? ReportFilePath { get; set; }

        public int CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public int? UpdatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool IsDeleted { get; set; }
        public int? DeletedBy { get; set; }
        public DateTime? DeletedAt { get; set; }
        public bool IsActive { get; set; }
    }

    public class AssetUtilizationDto
    {
        public int RunningAssets { get; set; }
        public double RunningPercentage { get; set; }
        public int UnderMaintenance { get; set; }
        public double MaintenancePercentage { get; set; }
        public int IdleAssets { get; set; }
        public double IdlePercentage { get; set; }
    }

    public class AssetCategoryDto
    {
        public string Category { get; set; }
        public int Count { get; set; }
        public double Utilization { get; set; }
    }

    public class RetirementPlanningDto
    {
        public int Next12Months { get; set; }
        public int TwoToFiveYears { get; set; }
        public int FivePlusYears { get; set; }
    }

    public class TopMaintenanceCostDto
    {
        public string AssetName { get; set; }
        public string AssetType { get; set; }
        public decimal YTDCost { get; set; }
        public int Frequency { get; set; }
        public string Trend { get; set; }
    }

    public class DashboardMetricsDto
    {
        public double OverallUptime { get; set; }
        public double ReliabilityIndex { get; set; }
        public double CostEfficiency { get; set; }
        public double ComplianceRate { get; set; }
    }

    public class DashboardSummaryDto
    {
        public AssetUtilizationDto AssetUtilization { get; set; }
        public List<AssetCategoryDto> AssetCategories { get; set; }
        public RetirementPlanningDto RetirementPlanning { get; set; }
        public List<TopMaintenanceCostDto> TopMaintenanceCosts { get; set; }
        public DashboardMetricsDto Metrics { get; set; }
        public AssetDashboardReportDto DowntimeMetrics { get; set; }
    }
    public class DashboardDataDto
    {
        public int TenantId { get; set; }
        public int LowCriticaAssetsCount { get; set; }
        public int HighCriticalAssetsCount { get; set; }
        public int MediumCriticalAssetsCount { get; set; }
        public int TotalActiveWorkOrder { get; set; }
        public int totalActiveTechnicians { get; set; }
    }
}
