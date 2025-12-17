using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace FactoryOpsApp.Domain.Entities.FactoryOpsTenants
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum AlertSeverityEnum
    {
        Low,
        Medium,
        High,
        Critical
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum AlertStatusEnum
    {
        Active,
        Resolved,
        Disabled
    }

    public class AlertRule
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int AlertRuleId { get; set; }

        [Required]
        public int TenantId { get; set; }

        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = null!;

        [Required]
        [MaxLength(500)]
        public string RuleCondition { get; set; } = null!;

        [Required]
        public AlertSeverityEnum Severity { get; set; }

        [Required]
        public AlertStatusEnum Status { get; set; } = AlertStatusEnum.Active;

        public DateTime? LastTriggered { get; set; }

        public int DevicesCount { get; set; }

        public bool IsActive { get; set; } = true;
        public bool IsDeleted { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? DeletedAt { get; set; }
        public int? CreatedBy { get; set; }
        public int? UpdatedBy { get; set; }
        public int? DeletedBy { get; set; }
    }
}