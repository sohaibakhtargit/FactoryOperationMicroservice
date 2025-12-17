using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FactoryOpsApp.Domain.Entities.FactoryOpsTenants
{
    public class MessagingTenantSettings
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int SettingsId { get; set; } 
        public int TenantId { get; set; }                                          
        public string? MqttBrokerUrl { get; set; }
        public int? MqttBrokerPort { get; set; }
        public string? MqttClientId { get; set; }
        public bool? CleanSession { get; set; }
        public int? KeepAlive { get; set; }
        public string? Username { get; set; }
        public string? Password { get; set; }
        public string? Topic { get; set; }
        public int? QoS { get; set; }
        public bool? OfflineBufferEnable { get; set; }
        public int? OfflineMax { get; set; }
        public string? KafkaTopicOverride { get; set; }
        public bool? UsePerTenantTopicOverride { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int? UpdatedByUserId { get; set; }
    }
}
