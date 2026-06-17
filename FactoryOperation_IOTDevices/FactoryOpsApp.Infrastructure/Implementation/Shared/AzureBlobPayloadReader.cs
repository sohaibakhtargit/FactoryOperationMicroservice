using Azure.Storage.Blobs;
using FactoryOperation_IOTDevices.FactoryOpsApp.Application.Interfaces.Shared;

namespace FactoryOperation_IOTDevices.FactoryOpsApp.Infrastructure.Implementation.Shared
{
    public class AzureBlobPayloadReader : IExternalPayloadReader
    {
        private readonly ILogger<AzureBlobPayloadReader> _logger;

        public AzureBlobPayloadReader(
            ILogger<AzureBlobPayloadReader> logger)
        {
            _logger = logger;
        }

        public async Task<byte[]> ReadAsync(
            string blobUrl,
            CancellationToken ct)
        {
            try
            {
                _logger.LogInformation(
                    "Downloading external payload from Blob: {Url}",
                    blobUrl);

                var blobClient = new BlobClient(new Uri(blobUrl));

                using var memory = new MemoryStream();
                await blobClient.DownloadToAsync(memory, ct);

                _logger.LogInformation(
                    "Downloaded {Size} bytes from Blob",
                    memory.Length);

                return memory.ToArray();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to download external payload from {Url}",
                    blobUrl);
                throw;
            }
        }
    }
}
