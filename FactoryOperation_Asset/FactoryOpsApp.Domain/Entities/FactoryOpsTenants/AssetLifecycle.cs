using FactoryOperation_Asset.FactoryOpsApp.Domain.Entities.FactoryOpsTenants;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
namespace FactoryOpsApp.Domain.Entities.FactoryOpsTenants

{

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum AssetLifecycleStageEnum
    {
        Acquisition,
        Operation,
        Maintenance,
        Retirement,
        Disposed
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]

    public enum AnalysisTypeEnum
    {
        RepairVsReplace,
        LifecycleCost,
        Depreciation,
        ROI

    }

    public class AssetLifecycle
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long LifecycleId { get; set; }
        public int TenantId { get; set; }
        public int AssetId { get; set; }
        [ForeignKey("AssetId")]
        public virtual AssetRegistry Asset { get; set; }
        [Required]
        [MaxLength(50)]

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

        public DateTime UpdatedOn { get; set; } = DateTime.UtcNow;


        public bool IsActive { get; set; } = true;

        public bool IsDeleted { get; set; } = false;

        public int? CreatedBy { get; set; }

        public int? UpdatedBy { get; set; }

        public int? DeletedBy { get; set; }

        public DateTime? DeletedAt { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

      public virtual ICollection<AssetLifecycleMappings>? AssetLifecycleMappings { get; set; }

    }

}
