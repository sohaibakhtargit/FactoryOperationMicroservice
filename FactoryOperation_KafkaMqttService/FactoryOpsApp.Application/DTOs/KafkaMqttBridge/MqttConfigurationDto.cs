using System.Text.Json;

namespace FactoryOperation_KafkaMqttService.FactoryOpsApp.Application.DTOs.KafkaMqttBridge
{
    public class MqttConfigurationDto
    {
        public int Id { get; set; }
        public int? TenantId { get; set; }
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
        public JsonDocument LastWill { get; set; } = JsonDocument.Parse("{}");
        public int SubscriptionQos { get; set; } = 1;
        public int PublishQos { get; set; } = 1;
        public JsonDocument OfflineBuffering { get; set; } = JsonDocument.Parse("{}");
        public bool IsActive { get; set; } = true;
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }
    }

    public class MqttConfigurationCreateDto
    {
        public int TenantId { get; set; }
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
        public string? LastWillJson { get; set; }
        public int SubscriptionQos { get; set; } = 1;
        public int PublishQos { get; set; } = 1;
        public string? OfflineBufferingJson { get; set; }
        public bool IsActive { get; set; } = true;
        public string? Description { get; set; }
        public string? CreatedBy { get; set; }
    }

    public class MqttConfigurationUpdateDto : MqttConfigurationCreateDto
    {
        public int Id { get; set; }
        public string? UpdatedBy { get; set; }
    }
}
