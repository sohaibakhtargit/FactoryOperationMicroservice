using FactoryOperation_KafkaMqttService.FactoryOpsApp.Shared.Models;

namespace FactoryOperation_KafkaMqttService.FactoryOpsApp.Shared.Interfaces
{
    public interface IKafkaConsumerBridgeService
    {
        Task ConsumeAsync(
        Func<KafkaMessageEnvelope, Task> handler,
        CancellationToken ct);
    }
}
