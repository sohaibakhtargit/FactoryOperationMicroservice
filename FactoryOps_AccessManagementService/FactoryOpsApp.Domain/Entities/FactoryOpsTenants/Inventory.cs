using FactoryOpsApp.Domain.Entities.FactoryOpsTenants;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace FactoryOpsApp.Domain.Entities
{
    [Table("Inventory")]
    public class Inventory
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ItemId { get; set; }
        [Required]
        public int TenantId { get; set; }
        [Required]
        [MaxLength(50)]
        public string ItemCode { get; set; } = string.Empty;
        [Required]
        [MaxLength(100)]
        public string ItemName { get; set; } = string.Empty;
        public string Manufacturer { get; set; } = string.Empty;
        public InventoryStatusEnum Status { get; set; } = InventoryStatusEnum.Active;
        public int? LocationId { get; set; }
        [ForeignKey("LocationId")]
        public Location? StorageLocation { get; set; }
        public InventoryCategoryEnum? Category { get; set; }
        [Required]
        public int QuantityAvailable { get; set; }
        [Required]
        public int ReorderLevel { get; set; }
        public int MaxStockLevel { get; set; } = 100;

        public int ReservedQuantity { get; set; } = 0;

        [Required]
        [Column(TypeName = "decimal(12,2)")]
        public decimal UnitPrice { get; set; }
        [Column(TypeName = "decimal(12,2)")]
        public decimal? MonthlyConsumption { get; set; }
        public bool IsActive { get; set; } = true;
        public bool IsDeleted { get; set; } = false;
        public int CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public int? UpdatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int? DeletedBy { get; set; }
        public DateTime? DeletedAt { get; set; }

        [NotMapped]
        public int AvailableQuantity => QuantityAvailable - ReservedQuantity;
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum InventoryStatusEnum
    {
        Active,
        InActive,
        Obsolete
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum InventoryCategoryEnum
    {
        Electrical,
        Hydraulic,
        Mechanical,
        Filtration
    }
}