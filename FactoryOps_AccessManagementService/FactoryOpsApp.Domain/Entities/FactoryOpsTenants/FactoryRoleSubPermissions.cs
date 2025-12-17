using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FactoryOpsApp.Domain.Entities.FactoryOpsTenants
{
    public class FactoryRoleSubPermissions
    {
        [Key]
        public int RoleSubPermissionId { get; set; }

        public int RoleId { get; set; }

        public int SubPermissionId { get; set; }

        public int? TenantId { get; set; }

        public bool IsActive { get; set; } = true;
        public bool IsDeleted { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public int? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int? UpdatedBy { get; set; }
        public DateTime? DeletedAt { get; set; }
        public int? DeletedBy { get; set; }

        // Navigation Properties (Optional but good)
        [ForeignKey("RoleId")]
        public FactoryRoles FactoryRoles { get; set; }

        [ForeignKey("SubPermissionId")]
        public FactorySubPermission FactorySubPermission { get; set; }
    }
}
