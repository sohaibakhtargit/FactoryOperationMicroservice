using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace FactoryOpsApp.Domain.Entities.FactoryOpsTenants
{
    public class AssetFinancialAnalysis
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long AnalysisId { get; set; }
        public int TenantId { get; set; }
        public int AssetId { get; set; }
        [ForeignKey("AssetId")]
        public virtual AssetRegistry Asset { get; set; }
        [Required]
        [MaxLength(50)]
        public string AnalysisType { get; set; } = string.Empty;
        public decimal? RepairCost { get; set; }
        public decimal? ReplacementCost { get; set; }
        public decimal? ROI { get; set; }
        public string? Recommendations { get; set; }
        public DateTime AnalysisDate { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;
        public bool IsDeleted { get; set; } = false;
        public int? CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}