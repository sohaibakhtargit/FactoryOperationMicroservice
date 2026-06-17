using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace FactoryOpsApp.Domain.Entities.FactoryOpsTenants
{
    public class FactoryRolePermissions
    {
        [Key]
        public int RolePermissionId { get; set; }
        public int RoleId { get; set; }
        [Required]
        public int TenantId { get; set; }
        public int PermissionId { get; set; }
        public bool IsActive { get; set; } = true;
        public bool IsDeleted { get; set; } = false;
        public DateTime CreatedAt { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int? UpdatedBy { get; set; }
        public DateTime? DeletedAt { get; set; }
        public int? DeletedBy { get; set; }
        public FactoryRoles FactoryRoles { get; set; } = null!;
        public FactoryPermission FactoryPermissions { get; set; } = null!;
    }
}
