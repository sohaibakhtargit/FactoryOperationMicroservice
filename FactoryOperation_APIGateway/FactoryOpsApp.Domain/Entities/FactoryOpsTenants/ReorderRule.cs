using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace FactoryOpsApp.Domain.Entities.FactoryOpsTenants
{
    [Table("ReorderRule")]
    public class ReorderRule
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ReorderRuleId { get; set; }

        [Required]
        public int TenantId { get; set; }

        [Required]
        public int InventoryId { get; set; }

        [ForeignKey("InventoryId")]
        public virtual Inventory? Inventory { get; set; }

        [Required]
        public int MinThreshold { get; set; }

        [Required]
        public int ReorderQuantity { get; set; }

        [Required]
        public int SupplierManagementId { get; set; }

        [ForeignKey("SupplierManagementId")]
        public virtual SupplierManagement? SupplierManagement { get; set; }

        public int LeadTimeDays { get; set; } = 0;

        [Column(TypeName = "decimal(18,2)")]
        public decimal LastPrice { get; set; } = 0;

        [Required]
        [MaxLength(20)]
        public ReorderPriority Priority { get; set; } = ReorderPriority.Medium;

        [Required]
        public bool AutoGenerateOrders { get; set; } = false;

        [Required]
        public bool IsActive { get; set; } = true;

        [Required]
        public bool IsDeleted { get; set; } = false;

        [Required]
        public int CreatedBy { get; set; }

        [Required]
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public int? UpdatedBy { get; set; }

        public DateTime? UpdatedDate { get; set; }

        public int? DeletedBy { get; set; }

        public DateTime? DeletedDate { get; set; }

        public virtual ICollection<PurchaseRequisition>? PurchaseRequisitions { get; set; }
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum ReorderPriority
    {
        Low,
        Medium,
        High,
        Critical
    }
}
