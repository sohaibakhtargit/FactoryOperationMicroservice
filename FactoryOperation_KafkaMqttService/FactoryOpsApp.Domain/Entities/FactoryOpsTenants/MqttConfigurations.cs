using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace FactoryOperation_KafkaMqttService.FactoryOpsApp.Domain.Entities.FactoryOpsTenants
{
    public class MqttConfigurations
    {
        [Key]
        public int Id { get; set; }
        public int? TenantId { get; set; }

        [Required]
        public string Name { get; set; } = null!;

        public string Environment { get; set; } = "Production";
        public string BrokerUrl { get; set; } = null!;
        public int BrokerPort { get; set; } = 1883;
        public string? ClientIdTemplate { get; set; }
        public bool CleanSession { get; set; } = true;
        public string? Username { get; set; }
        public string? Password { get; set; }
        public int KeepAliveSeconds { get; set; } = 30;
        public string? TopicTemplate { get; set; }

        public bool UseSsl { get; set; } = false;

        [Column(TypeName = "jsonb")]
        public JsonDocument LastWill { get; set; } = JsonDocument.Parse("{}");

        public int SubscriptionQos { get; set; } = 1;
        public int PublishQos { get; set; } = 1;

        [Column(TypeName = "jsonb")]
        public JsonDocument OfflineBuffering { get; set; } = JsonDocument.Parse("{}");

        public bool IsActive { get; set; } = true;
        public bool IsDeleted { get; set; } = false;        
        public string? CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string? UpdatedBy { get; set; }
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public string? Description { get; set; }
    }
}
