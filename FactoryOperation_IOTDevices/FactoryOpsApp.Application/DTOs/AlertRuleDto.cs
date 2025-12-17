using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FactoryOpsApp.Domain.Entities.FactoryOpsTenants;
using System.ComponentModel.DataAnnotations;

namespace FactoryOpsApp.Application.DTOs
{
    public class AlertRuleDto
    {
        public int AlertRuleId { get; set; }
        public int TenantId { get; set; }

        [Required]
        public string Name { get; set; } = null!;

        [Required]
        public string RuleCondition { get; set; } = null!;

        [Required]
        public AlertSeverityEnum Severity { get; set; }

        public AlertStatusEnum Status { get; set; } = AlertStatusEnum.Active;
        public int? CreatedBy { get; set; }
        public int? UpdatedBy { get; set; }
    }

    public class GetAlertRuleDto
    {
        public int AlertRuleId { get; set; }
        public int TenantId { get; set; }
        public string Name { get; set; } = null!;
        public string RuleCondition { get; set; } = null!;
        public AlertSeverityEnum Severity { get; set; }
        public AlertStatusEnum Status { get; set; }
        public DateTime? LastTriggered { get; set; }
        public int DevicesCount { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime CreatedAt { get; set; } 
        public DateTime UpdatedAt { get; set; } 
        public string LastTriggeredFormatted { get; set; } = string.Empty;
    }
}