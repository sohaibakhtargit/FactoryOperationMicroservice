using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FactoryOpsApp.Application.DTOs
{
    public class AddTenantDto
    {
        public int TenantId { get; set; }
        [Required(ErrorMessage = "Tenant Name is required.")]
        [DefaultValue("")]
        public string TenantName { get; set; } = string.Empty;
        [Required(ErrorMessage = "Domain or Subdomain is required.")]
        [DefaultValue("")]
        public string DomainOrSubdomain { get; set; }
        [Required(ErrorMessage = "Admin Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        [DefaultValue("")]
        public string AdminEmail { get; set; }
        public string? IndustryType { get; set; }
        public string Plan { get; set; }
        public bool Status { get; set; } = true;
        public int MaxUsers { get; set; }
        public int MaxAssets { get; set; }
        public double MaxStorage { get; set; }
        public DateTime? LastActiveDate { get; set; }
        public bool EnableBranding { get; set; } = false;
        public IFormFile? ImageFile { get; set; }
        public string TimeZone { get; set; }
        public string DefaultLanguage { get; set; }
        public DateTime CreatedAt { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int? UpdatedBy { get; set; }
        public DateTime? DeletedAt { get; set; }
        public int? DeletedBy { get; set; }
        public List<int>? ModuleIds { get; set; }
    }

    public class UpdateTenantModulesDto
    {
        public int TenantId { get; set; }
        public List<int> ModuleIds { get; set; } = new();
    }

}
