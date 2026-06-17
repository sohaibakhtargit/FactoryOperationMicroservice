using FactoryOperation_KafkaMqttService.FactoryOpsApp.Shared.Models;

namespace FactoryOperation_KafkaMqttService.FactoryOpsApp.Shared.Interfaces
{
    public interface IExternalPayloadStore
    {
        Task<ExternalPayloadReference> SaveAsync(
            byte[] payload,
            string correlationId,
            int? tenantId,
            CancellationToken ct);
    }

}
