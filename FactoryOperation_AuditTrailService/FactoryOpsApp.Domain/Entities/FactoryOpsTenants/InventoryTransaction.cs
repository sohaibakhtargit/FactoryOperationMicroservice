using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FactoryOpsApp.Domain.Entities.FactoryOpsTenants
{
    
    public class InventoryTransaction
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Required, MaxLength(20)]
        public string TransactionId { get; set; } = string.Empty;
        [Required]
        public int TenantId { get; set; }
        [Required]
        public TransactionType TransactionType { get; set; }
        [Required]
        public int PartId { get; set; }
        [Required]
        public int Quantity { get; set; }
        public int? FromLocationId { get; set; }
        public int? ToLocationId { get; set; }
        [MaxLength(100)]
        public string? ReferenceNumber { get; set; }
        public string? Notes { get; set; }
        [Required]
        public int PerformedById { get; set; }
        public TransactionStatus Status { get; set; } = TransactionStatus.Completed;
        public DateTime TransactionDate { get; set; } = DateTime.UtcNow;

        [ForeignKey("PartId")]
        public  Inventory Part { get; set; }
        [ForeignKey("FromLocationId")]
        public  Location FromLocation { get; set; }
        [ForeignKey("ToLocationId")]
        public  Location ToLocation { get; set; }
        [ForeignKey("PerformedById")]
        public  FactoryUsers PerformedBy { get; set; }

        public bool IsActive { get; set; } = true;
        public bool IsDeleted { get; set; } = false;
        public int? CreatedBy { get; set; }
        public int? UpdatedBy { get; set; }
        public int? DeletedBy { get; set; }
        public DateTime? DeletedAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }

    public enum TransactionType
    {
        Receipt,
        Issue,
        Transfer,
        Return,
        Adjustment,
        Scrap
    }

    public enum TransactionStatus
    {
        Pending,
        Completed,
        Cancelled,
        Rejected
    }
}
