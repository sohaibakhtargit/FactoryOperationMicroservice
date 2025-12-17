using System.Text.Json;

namespace FactoryOperation_KafkaMqttService.FactoryOpsApp.Application.DTOs.KafkaMqttBridge
{
    public class KafkaConfigurationDto
    {
        public int Id { get; set; }
        public int? TenantId { get; set; }
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
        public JsonDocument ProducerConfig { get; set; } = JsonDocument.Parse("{}");
        public JsonDocument ConsumerConfig { get; set; } = JsonDocument.Parse("{}");
        public JsonDocument DlqConfig { get; set; } = JsonDocument.Parse("{}");
        public bool IsActive { get; set; } = true;
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }
    }

    public class KafkaConfigurationCreateDto
    {
        public int TenantId { get; set; }
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
        public string? ProducerConfigJson { get; set; } // JSON as string
        public string? ConsumerConfigJson { get; set; }
        public string? DlqConfigJson { get; set; }
        public bool IsActive { get; set; } = true;
        public string? Description { get; set; }
        public string? CreatedBy { get; set; }
    }

    public class KafkaConfigurationUpdateDto : KafkaConfigurationCreateDto
    {
        public int Id { get; set; }
        public string? UpdatedBy { get; set; }
    }
}
