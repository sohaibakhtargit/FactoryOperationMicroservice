using FactoryOpsApp.Domain.Entities.FactoryOpsTenants;
using System.ComponentModel.DataAnnotations;

namespace FactoryOpsApp.Application.DTOs
{
    public class AssetLifecycleDto
    {
        public long LifecycleId { get; set; }

        [Required]
        public int AssetId { get; set; }

        [Required]
        public AssetLifecycleStageEnum Stage { get; set; }

        [Required]
        public DateTime AcquisitionDate { get; set; }

        public DateTime? ExpectedRetirementDate { get; set; }
        public DateTime? ActualRetirementDate { get; set; }
        public decimal? AcquisitionCost { get; set; }
        public decimal? CurrentValue { get; set; }
        public decimal? DepreciationValue { get; set; }
        public decimal? TCO { get; set; }
        public decimal? ROI { get; set; }
        public decimal? EstimatedRepairCost { get; set; }
        public decimal? ReplacementCost { get; set; }
        public decimal? AnnualMaintenanceCost { get; set; }
        public decimal? ResidualValue { get; set; }
        public string? AnalysisNotes { get; set; }
        public AnalysisTypeEnum? AnalysisType { get; set; }
        public DateTime? ReplacementDue { get; set; }
        public int TenantId { get; set; }
        public int? CreatedBy { get; set; }
        public int? UpdatedBy { get; set; }
    }

    public class GetAssetLifecycleDto
    {
        public long LifecycleId { get; set; }
        public int AssetId { get; set; }
        public string AssetName { get; set; } = string.Empty;
        public AssetLifecycleStageEnum Stage { get; set; }
        public DateTime AcquisitionDate { get; set; }
        public DateTime? ExpectedRetirementDate { get; set; }
        public DateTime? ActualRetirementDate { get; set; }
        public decimal? AcquisitionCost { get; set; }
        public decimal? CurrentValue { get; set; }
        public decimal? DepreciationValue { get; set; }
        public decimal? TCO { get; set; }
        public decimal? ROI { get; set; }
        public decimal? EstimatedRepairCost { get; set; }
        public decimal? ReplacementCost { get; set; }
        public decimal? AnnualMaintenanceCost { get; set; }
        public decimal? ResidualValue { get; set; }
        public string? AnalysisNotes { get; set; }
        public AnalysisTypeEnum? AnalysisType { get; set; }
        public DateTime? ReplacementDue { get; set; }
        public DateTime UpdatedOn { get; set; }
        public bool IsActive { get; set; }
        public string YearsInService { get; set; } = string.Empty;
        public string RetirementTimespan { get; set; } = string.Empty;
        public List<AssetLifecycleStageEnum> LifecycleStages { get; set; } = new List<AssetLifecycleStageEnum>();
    }

    public class AssetFinancialAnalysisDto
    {
        public long AnalysisId { get; set; }

        [Required]
        public int AssetId { get; set; }

        [Required]
        public string AnalysisType { get; set; } = string.Empty;

        public decimal? RepairCost { get; set; }
        public decimal? ReplacementCost { get; set; }
        public decimal? ROI { get; set; }
        public string? Recommendations { get; set; }
        public DateTime AnalysisDate { get; set; }
        public int TenantId { get; set; }
        public int? CreatedBy { get; set; }
    }

    public class GetAssetFinancialAnalysisDto
    {
        public long AnalysisId { get; set; }
        public int AssetId { get; set; }
        public string AssetName { get; set; } = string.Empty;
        public string AnalysisType { get; set; } = string.Empty;
        public decimal? RepairCost { get; set; }
        public decimal? ReplacementCost { get; set; }
        public decimal? ROI { get; set; }
        public string? Recommendations { get; set; }
        public DateTime AnalysisDate { get; set; }
        public bool IsActive { get; set; }
        public string AnalysisDateFormatted { get; set; } = string.Empty;
    }

    public class AssetLifecycleMetricsDto
    {
        public decimal TotalAssetValue { get; set; }
        public decimal TotalTCO { get; set; }
        public decimal AverageROI { get; set; }
        public int TotalAssets { get; set; }
        public int AssetsInOperation { get; set; }
        public int AssetsNeedingReplacement { get; set; }
    }

    public class AssetLifecycleFinancialSummaryDTO
    {
        public decimal TotalAssetValue { get; set; }
     //   public decimal AssetValueChangePercent { get; set; }

        public decimal TotalTCO { get; set; }
      //  public decimal TcoChangePercent { get; set; }

        public decimal AverageROI { get; set; }
    //    public decimal RoiChangePercent { get; set; }

        public int DueForReplacementCount { get; set; }
    }
    public class AssetLifeHistoryReportDTO
    {
        public int TenantId { get; set; }
        public int AssetId { get; set; }
        public string AssetName { get; set; } = string.Empty;
        public WorkOrderStatus? Status { get; set; }
        public int WorkOrderId { get; set; }
        public string WorkOrderName { get; set; } = string.Empty;
        public decimal? LaborCost { get; set; }
        public decimal? PartCost { get; set; }
        public decimal? TotalCost { get; set; }
    }

}
