using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FactoryOpsApp.Application.DTOs
{
    public class GetAllTenantsDto
    {
        public int TenantId { get; set; }
        public string TenantName { get; set; } = string.Empty;
        public string DomainOrSubdomain { get; set; }
        public string AdminEmail { get; set; }
        public string IndustryType { get; set; }
        public string Plan { get; set; }
        public bool Status { get; set; } = true;
        public bool Suspend { get; set; }
        public bool ForceLogout { get; set; }
        public int MaxUsers { get; set; }
        public int ActiveUsers { get; set; }
        public int MaxAssets { get; set; }
        public double MaxStorage { get; set; }
        public double? UseStorage { get; set; }
        public DateTime? LastActiveDate { get; set; }
        public bool EnableBranding { get; set; }
        public string? BrandingLogoUrl { get; set; }
        public string TimeZone { get; set; }
        public string DefaultLanguage { get; set; }
        public bool IsActive { get; set; } = true;
        public bool IsDeleted { get; set; } = false;
        public DateTime CreatedAt { get; set; }

        public List<ModuleDto>? Modules { get; set; }
    }
    public class ModuleDto
    {
        public int? ModuleId { get; set; }
        public string? ModuleName { get; set; }
    }

    public class ModulelistDto
    {
        public int? ModuleId { get; set; }
        public string? ModuleName { get; set; }
    }

}
