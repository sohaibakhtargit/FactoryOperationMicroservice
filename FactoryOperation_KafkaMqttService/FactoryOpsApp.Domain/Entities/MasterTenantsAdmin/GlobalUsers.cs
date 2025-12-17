using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FactoryOpsApp.Domain.Entities.MasterTenantsAdmin
{
    public class GlobalUsers
    {
        [Key]
        public int GlobalUserId { get; set; }

        [Required]
        [ForeignKey("FactoryTenants")]
        public int TenantId { get; set; }

        public FactoryTenants FactoryTenants { get; set; } = null!;

        [Required, MaxLength(255)]
        public string Email { get; set; }
        public string Password { get; set; }

        [Required]
        [EnumDataType(typeof(UserStatus))]
        public UserStatus Status { get; set; }

        public DateTime? LastLogin { get; set; }

        [MaxLength(45)]
        public string IpAddress { get; set; }

        [Column(TypeName = "jsonb")]
        public string Roles { get; set; }

        public int RoleId { get; set; }

        [ForeignKey("AdminLogins")]
        public int? SuspendedBy { get; set; }
        public bool Suspend { get; set; } = false;
        public bool ForceLogout { get; set; } = false;
        public AdminLogin AdminLogins { get; set; } = null!;
        public string? SuspensionReason { get; set; }
        public bool IsActive { get; set; } = true;
        public bool IsDeleted { get; set; } = false;
        public DateTime? CreatedAt { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int? UpdatedBy { get; set; }
        public DateTime? DeletedAt { get; set; }
        public int? DeletedBy { get; set; }

    }

    public enum UserStatus
    {
        Active,
        Suspended,
        Deleted
    }

}
