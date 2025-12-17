using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace FactoryOperation_KafkaMqttService.FactoryOpsApp.Domain.Entities.FactoryOpsTenants
{
    public class BridgeConfigurations
    {
        [Key]
        public int Id { get; set; }
        public int? TenantId { get; set; }
        [Required]
        public string Name { get; set; } = null!;
        public string Environment { get; set; } = "Production";
        public string Direction { get; set; } = "MqttToKafka"; // enum logic can be added
        public string SourcePattern { get; set; } = null!;
        public string TargetPattern { get; set; } = null!;

        [Column(TypeName = "jsonb")]
        public JsonDocument MappingRules { get; set; } = JsonDocument.Parse("{}");

        public string? Transformation { get; set; }
        public bool Enabled { get; set; } = true;
        public int Priority { get; set; } = 100;

        [Column(TypeName = "jsonb")]
        public JsonDocument RetryPolicy { get; set; } = JsonDocument.Parse("{}");
        public string? DlqTopic { get; set; }

        public bool IsActive { get; set; } = true;
        public bool IsDeleted { get; set; } = false;      
        public string? CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string? UpdatedBy { get; set; }
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public string? Description { get; set; }
    }
}
