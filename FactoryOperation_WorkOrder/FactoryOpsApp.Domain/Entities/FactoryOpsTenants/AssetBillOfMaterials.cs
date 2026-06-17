using FactoryOpsApp.Domain.Entities.FactoryOpsTenants;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FactoryOperation_WorkOrder.FactoryOpsApp.Domain.Entities.FactoryOpsTenants

{
    [Table("AssetBillOfMaterials")]
    public class AssetBillOfMaterials
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int BomPartId { get; set; }

        public int TenantId { get; set; }

        public int AssetId { get; set; }

        [ForeignKey("AssetId")]
        public AssetRegistry AssetRegistry { get; set; }

        [Required]
        [MaxLength(100)]
        public string PartNumber { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        public string PartName { get; set; } = string.Empty;

        public string? Description { get; set; }

        [MaxLength(100)]
        public string? Category { get; set; }

        public int Quantity { get; set; }

        public decimal? UnitCost { get; set; }

        public int? MinimumStockLevel { get; set; }

        public int? LeadTimeDays { get; set; }

        [MaxLength(150)]
        public string? Supplier { get; set; }

        [MaxLength(150)]
        public string? StorageLocation { get; set; }

        public string? CompatibleModels { get; set; }

        public bool IsActive { get; set; } = true;
        public bool IsDeleted { get; set; } = false;

        public int? CreatedBy { get; set; }
        public int? UpdatedBy { get; set; }
        public int? DeletedBy { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? DeletedAt { get; set; }
    }
}

