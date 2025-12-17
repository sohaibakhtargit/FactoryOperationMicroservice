using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FactoryOpsApp.Domain.Entities.FactoryOpsTenants
{
    public enum IntegrationSettingsCategory
    {
        General,
        Notifications,
        Integrations
    }

    [Table("IntegrationSettings")]
    public class IntegrationSettings
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int IntegrationId { get; set; }

        public int TenantId { get; set; }
        [Required]
        [MaxLength(255)]
        public string Name { get; set; } = string.Empty;

        [Required]
        public IntegrationSettingsCategory Category { get; set; }

        public string? Description { get; set; }

        [Required]
        [Column(TypeName = "jsonb")]
        public SettingValueObject SettingValue { get; set; } = new SettingValueObject();
        
        public bool IsActive { get; set; } = false;
        public bool IsDeleted { get; set; } = false;
        public int CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public int? UpdatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int? DeletedBy { get; set; }
        public DateTime? DeletedAt { get; set; }
    }
    public class SettingValueObject
    {
        public string? ApiKey { get; set; }
        public string? BaseUrl { get; set; }
        public string? Endpoint { get; set; }
        public string? Username { get; set; }
        public string? Password { get; set; }
        public bool? EnableLogging { get; set; }
        public Dictionary<string, string>? ExtraSettings { get; set; }
    }

}
