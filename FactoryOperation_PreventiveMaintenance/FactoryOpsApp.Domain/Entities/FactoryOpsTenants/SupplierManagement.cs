using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FactoryOpsApp.Domain.Entities.FactoryOpsTenants
{
    [Table("SupplierManagement")]
    public class SupplierManagement
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int SupplierManagementId { get; set; }

        public int? TenantId { get; set; }

        [Required]
        [MaxLength(255)]
        public string SupplierName { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string SupplierCode { get; set; } = string.Empty;

        [MaxLength(255)]
        public string? ContactPerson { get; set; }

        [MaxLength(255)]
        public string? Email { get; set; }

        [MaxLength(50)]
        public string? Phone { get; set; }

        public string? Address { get; set; }

        [Required]
        public int LeadTimeDays { get; set; }

        [Column(TypeName = "decimal(3,2)")]
        public decimal? PerformanceRating { get; set; }

        [Column(TypeName = "decimal(15,2)")]
        public decimal? LastPrice { get; set; }

        [Required]
        public bool IsActive { get; set; } = true;

        [Required]
        public bool IsDeleted { get; set; } = false;

        [Required]
        public int CreatedBy { get; set; }

       
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public int? UpdatedBy { get; set; }

        public DateTime? UpdatedDate { get; set; }

        public int? DeletedBy { get; set; }

        public DateTime? DeletedDate { get; set; }

        public virtual ICollection<ReorderRule>? ReorderRules { get; set; }
        public virtual ICollection<PurchaseRequisition>? PurchaseRequisitions { get; set; }
    }
}
