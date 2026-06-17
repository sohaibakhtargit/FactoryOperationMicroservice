using FactoryOperation_KafkaMqttService.FactoryOpsApp.Shared.Models;

namespace FactoryOperation_KafkaMqttService.FactoryOpsApp.Messaging.Interfaces
{
    public interface ITenantResolver
    {
        int? ResolveFromMqtt(string topic);
        int? ResolveFromKafka(KafkaMessageEnvelope envelope);
    }

}
