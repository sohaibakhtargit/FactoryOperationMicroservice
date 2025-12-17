using FactoryOpsApp.Domain.Entities;
using FactoryOpsApp.Domain.Entities.FactoryOpsTenants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FactoryOpsApp.Application.DTOs
{
    public class IntegrationSettingsDto
    {
        public int IntegrationId { get; set; }
        public int TenantId { get; set; }
        public string Name { get; set; } = string.Empty;
        public IntegrationSettingsCategory Category { get; set; }
        public string? Description { get; set; }
        public SettingValueObject SettingValue { get; set; } = new SettingValueObject();
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateIntegrationSettingsDto
    {
        public int TenantId { get; set; }

        public string Name { get; set; } = string.Empty;
        public IntegrationSettingsCategory Category { get; set; }
        public string? Description { get; set; }
        public SettingValueObject SettingValue { get; set; } = new SettingValueObject();
        public bool? IsActive { get; set; } = true;
        public int CreatedBy { get; set; }
    }

    public class UpdateIntegrationSettingsDto
    {
        public int IntegrationId { get; set; }
        public string? Name { get; set; }
        public IntegrationSettingsCategory? Category { get; set; }
        public string? Description { get; set; }
        public SettingValueObject? SettingValue { get; set; }
        public bool? IsActive { get; set; } = true;
        public int UpdatedBy { get; set; }
    }
}
