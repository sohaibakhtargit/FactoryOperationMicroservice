using FactoryOpsApp.Domain.Entities.FactoryOpsTenants;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FactoryOpsApp.Domain.Entities.MasterTenantsAdmin
{
    public class FactoryTenants
    {
        [Key]
        public int TenantId { get; set; }
        public string TenantName { get; set; } = string.Empty;
        public string DomainOrSubdomain { get; set; }
        public string AdminEmail { get; set; }
        public string IndustryType { get; set; }
        public string Plan { get; set; }
        public bool Status { get; set; } = true;
        public int MaxUsers { get; set; }
        public int MaxAssets { get; set; }
        public double MaxStorage { get; set; }
        public bool Suspend { get; set; } = false;
        public bool ForceLogout { get; set; } = false;
        public DateTime? LastActiveDate { get; set; }
        public bool EnableBranding { get; set; } = false;
        public byte[]? ImageName { get; set; }
        public string? BrandingLogoUrl { get; set; }
        public string TimeZone { get; set; }
        public string DefaultLanguage { get; set; }
        public string? Bio { get; set; }
        public bool IsActive { get; set; } = true;
        public bool IsDeleted { get; set; } = false;
        public DateTime CreatedAt { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int? UpdatedBy { get; set; }
        public DateTime? DeletedAt { get; set; }
        public int? DeletedBy { get; set; }
        public string? DatabaseName { get; set; }
        public string? ContactNumber { get; set; }
        public string? ProfileImage { get; set; }
        public string? ProfileImageURL { get; set; }
    }
}
