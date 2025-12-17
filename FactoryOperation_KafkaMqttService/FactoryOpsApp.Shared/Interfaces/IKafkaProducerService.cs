using FactoryOperation_KafkaMqttService.FactoryOpsApp.Shared.Models;

namespace FactoryOperation_KafkaMqttService.FactoryOpsApp.Shared.Interfaces
{
    public interface IKafkaProducerService : IDisposable
    {
        Task ProduceAsync(string topic, string key, byte[] payload, CancellationToken ct = default);
        Task ProduceEnvelopeAsync(string topic, KafkaMessageEnvelope envelope, CancellationToken ct = default);

    }
}
