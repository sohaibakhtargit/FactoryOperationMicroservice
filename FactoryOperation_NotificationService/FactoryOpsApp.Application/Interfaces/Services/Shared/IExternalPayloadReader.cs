namespace FactoryOperation_NotificationService.FactoryOpsApp.Application.Interfaces.Services.Shared
{
    public interface IExternalPayloadReader
    {
        Task<byte[]> ReadAsync(
            string blobUrl,
            CancellationToken ct);
    }
}
