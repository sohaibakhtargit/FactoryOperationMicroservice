using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FactoryOpsApp.Domain.Entities.FactoryOpsTenants
{
    using System.ComponentModel.DataAnnotations.Schema;

    public class FactoryRoles
    {
        [Key]
        public int RoleId { get; set; }
        public int? TenantId { get; set; }
        public string RoleName { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public bool IsDeleted { get; set; } = false;
        public DateTime CreatedAt { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int? UpdatedBy { get; set; }
        public DateTime? DeletedAt { get; set; }
        public int? DeletedBy { get; set; }

        public ICollection<FactoryUserRoles> FactoryUserRoles { get; set; } = new List<FactoryUserRoles>();
        public ICollection<FactoryRolePermissions> FactoryRolePermissions { get; set; } = new List<FactoryRolePermissions>();
    }

}
