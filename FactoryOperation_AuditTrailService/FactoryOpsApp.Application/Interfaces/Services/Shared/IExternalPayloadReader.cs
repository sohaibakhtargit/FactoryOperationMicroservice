namespace FactoryOperation_AuditTrailService.FactoryOpsApp.Application.Interfaces.Services.Shared
{
    public interface IExternalPayloadReader
    {
        Task<byte[]> ReadAsync(
            string blobUrl,
            CancellationToken ct);
    }
}
