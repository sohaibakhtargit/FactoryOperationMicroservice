using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FactoryOpsApp.Domain.Entities.MasterTenantsAdmin
{
    [Table("announcement_tenants")]
    public class AnnouncementTenant
    {
        [Key]
        [Column("mapping_id")]
        public int MappingId { get; set; }

        [ForeignKey("Announcement")]
        [Column("announcement_id")]
        public int AnnouncementId { get; set; }

        [Column("tenant_id")]
        public int? TenantId { get; set; }
        [ForeignKey("TenantId")]
        public virtual FactoryTenants Tenant { get; set; }
        public virtual Announcement Announcement { get; set; }
        public bool IsActive { get; set; } = true;
        public bool IsDeleted { get; set; } = false;
        public DateTime? CreatedAt { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int? UpdatedBy { get; set; }
        public DateTime? DeletedAt { get; set; }
        public int? DeletedBy { get; set; }

    }
}
