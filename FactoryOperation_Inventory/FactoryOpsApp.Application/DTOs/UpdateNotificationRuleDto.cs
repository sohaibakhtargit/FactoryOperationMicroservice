using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FactoryOpsApp.Application.DTOs
{
    public class UpdateNotificationRuleDto
    {
        public int RuleId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string TriggerEvent { get; set; } = string.Empty;
        public string Channel { get; set; } = string.Empty;
        public string Recipient { get; set; } = string.Empty;
        public bool IsEnabled { get; set; }
    }

}
