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
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum TopicTypeEnum
    {
        Sensor,
        Control,
        Status,
        Command,
        Event,
        Alert
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum DataTypeEnum
    {
        JSON,
        XML,
        Text,
        Binary,
        Custom,
        Number,
        String,
        Boolean
    }

    [Table("MqttTopics")]
    public class FactoryMqttTopic
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int TopicId { get; set; }

        [Required]
        public string Name { get; set; } = null!;

        [Required]
        public int TenantId { get; set; }

        public string? MqttPath { get; set; }

        [Required]
        public TopicTypeEnum Type { get; set; }

        
      //  public DataTypeEnum DataType { get; set; } = DataTypeEnum.JSON;

        public int QoS { get; set; }

        public int DeviceCount { get; set; } = 0;

        public DateTime? LastMessage { get; set; }

        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        public bool IsDeleted { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? DeletedAt { get; set; }

        public int? CreatedBy { get; set; }

        public int? UpdatedBy { get; set; }

        public int? DeletedBy { get; set; }

        // Navigation properties

        [JsonIgnore]
        public virtual ICollection<FactoryDeviceTopic>? DeviceTopics { get; set; }

        public virtual ICollection<TopicSchemaDefinition> SchemaDefinitions { get; set; } = new List<TopicSchemaDefinition>();
    }
}