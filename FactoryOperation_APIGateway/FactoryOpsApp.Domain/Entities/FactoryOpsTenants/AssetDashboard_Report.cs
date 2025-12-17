using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace FactoryOpsApp.Domain.Entities.FactoryOpsTenants
{
    public class AssetDashboard_Report
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int DashboardId { get; set; }
        public int TenantId { get; set; }

        [Required]
        public DateTime PeriodDate { get; set; }
        [Required]
        [MaxLength(20)]
        public string PeriodType { get; set; } = "Monthly";
        public decimal OverallUptime { get; set; }
        public decimal UptimeTarget { get; set; }
        public decimal ReliabilityIndex { get; set; }
        public decimal ReliabilityTarget { get; set; }
        public decimal CostEfficiency { get; set; }
        public decimal CostEfficiencyTarget { get; set; }
        public decimal ComplianceRate { get; set; }
        public decimal ComplianceTarget { get; set; }
        public int RunningAssets { get; set; }
        public int MaintenanceAssets { get; set; }
        public int IdleAssets { get; set; }
        public int Next12MonthsReplacements { get; set; }
        public int TwoToFiveYearsReplacements { get; set; }
        public int FivePlusYearsReplacements { get; set; }
        [MaxLength(50)]
        public ReportType? SelectedReportType { get; set; }

        [MaxLength(50)]
        public DateRangeOption? SelectedDateRangeOption { get; set; }

        public DateTime? CustomStartDate { get; set; }
        public DateTime? CustomEndDate { get; set; }

        [MaxLength(50)]
        public string? SelectedAssetFilter { get; set; } 

        [MaxLength(500)]
        public string? SelectedSpecificAssets { get; set; }

        [MaxLength(20)]
        public ExportFormat? SelectedExportFormat { get; set; }

        [MaxLength(100)]
        public string? ReportName { get; set; }

        [MaxLength(20)]
        public string ReportStatus { get; set; } = "Draft";

        public DateTime? ReportGeneratedAt { get; set; }

        [MaxLength(500)]
        public string? ReportFilePath { get; set; }

        public int CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public int? UpdatedBy { get; set; }
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public bool IsDeleted { get; set; } = false;
        public int? DeletedBy { get; set; }
        public DateTime? DeletedAt { get; set; }
        public bool IsActive { get; set; } = true;
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum ReportType
    {
        AssetUtilization,
        MaintenanceSummary,
        FinancialAnalysis,
        ComplianceStatus,
        PerformanceKPIs,
        LifecycleAnalysis
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum DateRangeOption
    {
        Last30Days,
        LastQuarter,
        LastYear,
        YearToDate,
        CustomRange
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum ExportFormat
    {
        PDFReport,
        ExcelSpreadsheet,
        CSVData,
        InteractiveDashboard
    }

    namespace FactoryOpsApp.Application.DTOs
    {
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


        }
    }

}