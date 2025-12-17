using System.Text.Json;

namespace FactoryOperation_KafkaMqttService.FactoryOpsApp.Application.DTOs.KafkaMqttBridge
{
    public class BridgeConfigurationDto
    {
        public int Id { get; set; }
        public int? TenantId { get; set; }
        public string Name { get; set; } = null!;
        public string Environment { get; set; } = "Production";
        public string Direction { get; set; } = "MqttToKafka";
        public string SourcePattern { get; set; } = null!;
        public string TargetPattern { get; set; } = null!;
        public JsonDocument MappingRules { get; set; } = JsonDocument.Parse("{}");
        public string? Transformation { get; set; }
        public bool Enabled { get; set; } = true;
        public int Priority { get; set; } = 100;
        public JsonDocument RetryPolicy { get; set; } = JsonDocument.Parse("{}");
        public string? DlqTopic { get; set; }
        public bool IsActive { get; set; } = true;
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }
    }

    public class BridgeConfigurationCreateDto
    {
        public int TenantId { get; set; }
        public string Name { get; set; } = null!;
        public string Environment { get; set; } = "Production";
        public string Direction { get; set; } = "MqttToKafka";
        public string SourcePattern { get; set; } = null!;
        public string TargetPattern { get; set; } = null!;
        public string? MappingRulesJson { get; set; }
        public string? Transformation { get; set; }
        public bool Enabled { get; set; } = true;
        public int Priority { get; set; } = 100;
        public string? RetryPolicyJson { get; set; }
        public string? DlqTopic { get; set; }
        public bool IsActive { get; set; } = true;
        public string? Description { get; set; }
        public string? CreatedBy { get; set; }
    }

    public class BridgeConfigurationUpdateDto : BridgeConfigurationCreateDto
    {
        public int Id { get; set; }
        public string? UpdatedBy { get; set; }
    }
}
