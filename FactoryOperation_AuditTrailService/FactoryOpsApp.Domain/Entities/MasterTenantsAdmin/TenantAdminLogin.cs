using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FactoryOpsApp.Domain.Entities.MasterTenantsAdmin
{
    public class TenantAdminLogin
    {
        [Key]
        public int Id { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        [ForeignKey("AdminRoles")]
        public int RoleId { get; set; }
        [ForeignKey("FactoryTenants")]
        public int TenantId { get; set; }
        public FactoryTenants FactoryTenants { get; set; } = null!;
        public AdminRoles AdminRoles { get; set; } = null!;
        public bool IsActive { get; set; } = true;
        public bool IsDeleted { get; set; } = false;
        public bool Status { get; set; } = true;
        public bool Suspend { get; set; } = false;
        public bool ForceLogout { get; set; } = false;
        public DateTime CreatedAt { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int? UpdatedBy { get; set; }
        public DateTime? DeletedAt { get; set; }
        public int? DeletedBy { get; set; }
    }
}
