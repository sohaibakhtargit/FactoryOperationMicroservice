using FactoryOpsApp.Domain.Entities.FactoryOpsTenants;

namespace FactoryOpsApp.Application.DTOs
{
    public class MqttTopicDto
    {
        public int TopicId { get; set; }
        public string Name { get; set; } = null!;
        public int TenantId { get; set; }
        public string? MqttPath { get; set; }
        public TopicTypeEnum Type { get; set; }
        public int QoS { get; set; }
        public string? Description { get; set; }
        public List<TopicSchemaDefinitionDto> SchemaDefinitions { get; set; } = new List<TopicSchemaDefinitionDto>();
    }

    public class TopicSchemaDefinitionDto
    {
        public string KeyName { get; set; } = string.Empty;
        public DataTypeEnum DataType { get; set; } = DataTypeEnum.JSON;
    }

}