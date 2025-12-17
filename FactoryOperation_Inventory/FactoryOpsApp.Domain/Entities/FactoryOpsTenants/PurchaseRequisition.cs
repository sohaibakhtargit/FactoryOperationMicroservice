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
    [Table("PurchaseRequisition")]
    public class PurchaseRequisition
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int PurchaseRequisitionId { get; set; }

        [Required]
        public int TenantId { get; set; }

        [Required]
        [MaxLength(50)]
        public string RequisitionId { get; set; } = string.Empty;

        [Required]
        public int ReorderRuleId { get; set; }

        [ForeignKey("ReorderRuleId")]
        public virtual ReorderRule? ReorderRule { get; set; }

        [Required]
        public int InventoryId { get; set; }

        [ForeignKey("InventoryId")]
        public virtual Inventory? Inventory { get; set; }

        [Required]
        public int Quantity { get; set; }

        [Required]
        public int SupplierManagementId { get; set; }

        [ForeignKey("SupplierManagementId")]
        public virtual SupplierManagement? SupplierManagement { get; set; }

        [Required]
        [Column(TypeName = "decimal(15,2)")]
        public decimal EstimatedCost { get; set; }

        [Required]
        [MaxLength(20)]
        public ReorderPriority Priority { get; set; } = ReorderPriority.Medium;

        [Required]
        public DateOnly GeneratedDate { get; set; }

        [Required]
        public DateOnly ExpectedDeliveryDate { get; set; }

        [Required]
        [MaxLength(20)]
        public RequisitionStatus Status { get; set; } = RequisitionStatus.Pending;

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

        public ManagerAprovalStatus ManagerAprovalStatus { get; set; } = ManagerAprovalStatus.Pending;

        public SupplierAcceptanceStatus SupplierAcceptanceStatus { get; set; } = SupplierAcceptanceStatus.Pending;

        [MaxLength(500)]
        public string? SupplierComment { get; set; }
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum RequisitionStatus
    {
        Pending,
        Approved,
        Ordered,
        Delivered,
        Cancelled
    }
    public enum ManagerAprovalStatus
    {
        Pending,
        Approved,
        Cancelled
    }
    public enum SupplierAcceptanceStatus
    {
        Pending,
        Accepted,
        Cancelled
    }
}
