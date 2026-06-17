using System.ComponentModel.DataAnnotations;

namespace FactoryOpsApp.Domain.Entities.FactoryOpsTenants
{
    public class FactoryUserRoles
    {
        [Key]
        public int UserRoleId { get; set; }
        public int UserId { get; set; }
        public int RoleId { get; set; }
        public bool IsActive { get; set; } = true;
        public bool IsDeleted { get; set; } = false;
        public DateTime CreatedAt { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int? UpdatedBy { get; set; }
        public DateTime? DeletedAt { get; set; }
        public int? DeletedBy { get; set; }
        [Required]
        public int TenantId { get; set; }
        public FactoryUsers FactoryUsers { get; set; } = null!;
        public FactoryRoles FactoryRoles { get; set; } = null!;

    }
}
