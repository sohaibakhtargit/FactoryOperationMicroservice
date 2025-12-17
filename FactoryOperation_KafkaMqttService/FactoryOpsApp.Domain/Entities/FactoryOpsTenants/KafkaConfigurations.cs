using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace FactoryOperation_KafkaMqttService.FactoryOpsApp.Domain.Entities.FactoryOpsTenants
{
    public class KafkaConfigurations
    {
        [Key]
        public int Id { get; set; }
        public int? TenantId { get; set; }

        [Required]
        public string Name { get; set; } = null!;

        public string Environment { get; set; } = "Production";
        public string BootstrapServers { get; set; } = null!;
        public string? GroupId { get; set; }
        public string? TopicPattern { get; set; }
        public bool UsePerTenantTopic { get; set; }
        public bool EnableAutoOffsetStore { get; set; }
        public bool EnableAutoCommit { get; set; } = true;
        public string? SecurityProtocol { get; set; }
        public string? SaslMechanism { get; set; }
        public string? SaslUsername { get; set; }
        public string? SaslPassword { get; set; }

        // JSON columns
        [Column(TypeName = "jsonb")]
        public JsonDocument ProducerConfig { get; set; } = JsonDocument.Parse("{}");

        [Column(TypeName = "jsonb")]
        public JsonDocument ConsumerConfig { get; set; } = JsonDocument.Parse("{}");

        [Column(TypeName = "jsonb")]
        public JsonDocument DlqConfig { get; set; } = JsonDocument.Parse("{}");

        public bool IsActive { get; set; } = true;
        public bool IsDeleted { get; set; } = false;
      
        public string? CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string? UpdatedBy { get; set; }
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public string? Description { get; set; }
    }
}
