using FactoryOperation_WorkOrder.FactoryOpsApp.Domain.Entities.FactoryOpsTenants;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FactoryOpsApp.Domain.Entities.FactoryOpsTenants
{
    public class AssetRegistry
    {
        public enum CriticalityLevel
        {
            Low,
            Medium,
            High
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int AssetId { get; set; }
        public int TenantId { get; set; }

        [Required]
        [MaxLength(150)]
        public string AssetName { get; set; } = string.Empty;

        public int? AssetTypeId { get; set; }
        [ForeignKey("AssetTypeId")]
        public FactoryAssetType? FactoryAssetType { get; set; }

        public string? Model { get; set; }
        public string? SerialNumber { get; set; }
        public string? AssetUniqueId { get; set; }
        public string? CategoryHierarchy { get; set; }

        public int LocationId { get; set; }
        [ForeignKey("LocationId")]
        public Location Location { get; set; }

        public string? Department { get; set; }
        public string? Vendor { get; set; }
        public string? Supplier { get; set; }
        public string? Manufacturer { get; set; }
        public DateTime? PurchaseDate { get; set; }
        public decimal? AcquisitionCost { get; set; }
        public DateTime? WarrantyExpiry { get; set; }
        public int? ExpectedLifespan { get; set; }
        public string? DepreciationRule { get; set; }

        [MaxLength(50)]
        public string? Power { get; set; }

        [MaxLength(10)]
        public CriticalityLevel? Criticality { get; set; }

        [MaxLength(500)]
        [Column("DocumentUrl")]
        public string? DocumentUrl { get; set; }
        [Column("documentfile")]
        public string? DocumentFile { get; set; }
        public string? InsurancePolicyNumber { get; set; }

        public bool IsActive { get; set; } = true;
        public bool IsDeleted { get; set; } = false;
        public int? CreatedBy { get; set; }
        public int? UpdatedBy { get; set; }
        public int? DeletedBy { get; set; }
        public DateTime? DeletedAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public int? BulkImportId { get; set; }

        [ForeignKey("BulkImportId")]
        public AssetBulkImport? BulkImport { get; set; }

        // Navigation collections
        public ICollection<AssetTracking> AssetTracking { get; set; }
        public ICollection<MaintenanceHistory> MaintenanceHistory { get; set; }
        public ICollection<AssetLifecycle> AssetLifecycles { get; set; }
        public ICollection<AssetDocuments> AssetDocuments { get; set; }
        public ICollection<AssetFinancialAnalysis> AssetFinancialAnalysis { get; set; }
        public ICollection<AssetDashboard_Report> AssetDashboard_Reports { get; set; }
        public ICollection<AssetBillOfMaterials> BillOfMaterials { get; set; }

    }
}
