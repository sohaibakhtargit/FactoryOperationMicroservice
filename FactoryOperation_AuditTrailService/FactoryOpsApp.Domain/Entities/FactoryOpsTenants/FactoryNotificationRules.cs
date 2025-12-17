using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace FactoryOpsApp.Domain.Entities.FactoryOpsTenants
{
    [Table("Factory_Notification_Rules")]
    public class FactoryNotificationRules
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int RuleId { get; set; }

        [Required]
        public int TenantId { get; set; }

        [Required, MaxLength(255)]
        public string Name { get; set; }

        [Required]
        public TriggerEvent TriggerEvent { get; set; }

        [Required]
        public DeliveryMethod DeliveryMethod { get; set; }

        [Required]
        public RecipientType RecipientType { get; set; }

        [Required, MaxLength(255)]
        public string RecipientId { get; set; }
        public int? EscalationTime { get; set; }

        [MaxLength(255)]
        public string? EscalationRecipient { get; set; }

        public bool IsActive { get; set; } = true;
        public bool IsDeleted { get; set; } = false;

        public DateTime? CreatedAt { get; set; }
        public int? CreatedBy { get; set; }

        public DateTime? UpdatedAt { get; set; }
        public int? UpdatedBy { get; set; }

        public DateTime? DeletedAt { get; set; }
        public int? DeletedBy { get; set; }
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum TriggerEvent
    {
        WorkOrderCreated,
        MaintenanceDue,
        QualityAlert,   
        SafetyIncident,  
        AssetDown
    }
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum DeliveryMethod
    {
        Email,
        SMS,
        InApp
    }
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum RecipientType
    {
        Role,
        Team,
        SpecificUser
    }
}
