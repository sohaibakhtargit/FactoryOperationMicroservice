using FactoryOpsApp.Domain.Entities.FactoryOpsTenants;

namespace FactoryOpsApp.Application.DTOs
{
    public class NotificationRuleDto
    {
        public int RuleId { get; set; }
        public int TenantId { get; set; }
        public string Name { get; set; }
        public TriggerEvent TriggerEvent { get; set; }
        public DeliveryMethod DeliveryMethod { get; set; }
        public RecipientType RecipientType { get; set; }
        public string? RecipientId { get; set; }
        public int? EscalationTime { get; set; }
        public string? EscalationRecipient { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? CreatedAt { get; set; }
        public int? CreatedBy { get; set; }

        public DateTime? UpdatedAt { get; set; }
        public int? UpdatedBy { get; set; }

        public DateTime? DeletedAt { get; set; }
        public int? DeletedBy { get; set; }

    }

}
