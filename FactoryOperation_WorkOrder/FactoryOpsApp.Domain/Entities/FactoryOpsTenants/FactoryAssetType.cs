using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FactoryOpsApp.Domain.Entities.FactoryOpsTenants
{
    [Table("FactoryAssetTypes")]
    public class FactoryAssetType
    {
        [Key]
        [Column("AssetTypeId")]
        public int AssetTypeId { get; set; }

        [Required]
        public int TenantId { get; set; }

        [Required, MaxLength(100)]
        public string Type_Name { get; set; } = null!;

        [Column("Description")]
        public string? Description { get; set; }
        [Column(TypeName = "numeric(5,2)")]
        public decimal? Default_Depreciation_Rate { get; set; }

        [Column("IsActive")]
        public bool IsActive { get; set; } = true;

        [Column("IsDeleted")]
        public bool IsDeleted { get; set; } = false;

        [Column("CreatedBy")]
        public int? CreatedBy { get; set; }

        [Column("UpdatedBy")]
        public int? UpdatedBy { get; set; }

        [Column("DeletedBy")]
        public int? DeletedBy { get; set; }

        [Column("DeletedAt")]
        public DateTime? DeletedAt { get; set; }

        [Column("CreatedAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("UpdatedAt")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<AssetRegistry> Assets { get; set; }
    }
}
