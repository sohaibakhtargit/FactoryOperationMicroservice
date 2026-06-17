using FactoryOperation_IOTDevices.FactoryOpsApp.Application.Models;

namespace FactoryOperation_IOTDevices.FactoryOpsApp.Application.Interfaces.Handlers
{
    public interface IKafkaTelemetryHandler
    {
        Task HandleTelemetryAsync(KafkaMessageEnvelope envelope);
        Task HandleStatusAsync(KafkaMessageEnvelope envelope);
        Task HandleAlertAsync(KafkaMessageEnvelope envelope);
    }
}
