namespace FactoryOperation_IOTDevices.FactoryOpsApp.Application.Interfaces.Shared
{
    public interface IExternalPayloadReader
    {
        Task<byte[]> ReadAsync(
            string blobUrl,
            CancellationToken ct);
    }
}
