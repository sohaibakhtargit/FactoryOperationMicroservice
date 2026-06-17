using Azure.Storage.Blobs;
using FactoryOperation_KafkaMqttService.FactoryOpsApp.Shared.Config;
using FactoryOperation_KafkaMqttService.FactoryOpsApp.Shared.Interfaces;
using FactoryOperation_KafkaMqttService.FactoryOpsApp.Shared.Models;
using Microsoft.Extensions.Options;

namespace FactoryOperation_KafkaMqttService.FactoryOpsApp.Shared.Services
{
    public class AzureBlobPayloadStore : IExternalPayloadStore
    {
        private readonly BlobContainerClient _container;
        private readonly BlobStorageSettings _settings;

        public AzureBlobPayloadStore(IOptions<BlobStorageSettings> options)
        {
            _settings = options.Value
                ?? throw new ArgumentNullException(nameof(options));

            if (string.IsNullOrWhiteSpace(_settings.ConnectionString))
                throw new InvalidOperationException("BlobStorage:ConnectionString is not configured");

            if (string.IsNullOrWhiteSpace(_settings.ContainerName))
                throw new InvalidOperationException("BlobStorage:ContainerName is not configured");

            // Create client
            var serviceClient = new BlobServiceClient(_settings.ConnectionString);
            _container = serviceClient.GetBlobContainerClient(_settings.ContainerName);

            // Ensure container exists (safe for prod + dev)
            _container.CreateIfNotExists();
        }

        public async Task<ExternalPayloadReference> SaveAsync(
            byte[] payload,
            string correlationId,
            int? tenantId,
            CancellationToken ct)
        {
            if (payload == null || payload.Length == 0)
                throw new ArgumentException("Payload cannot be null or empty", nameof(payload));

            var blobName =
                $"{tenantId ?? 0}/" +
                $"{DateTime.UtcNow:yyyy/MM/dd}/" +
                $"{correlationId}-{Guid.NewGuid():N}.json";

            var blob = _container.GetBlobClient(blobName);

            using var stream = new MemoryStream(payload);
            await blob.UploadAsync(stream, overwrite: true, cancellationToken: ct);

            return new ExternalPayloadReference
            {
                BlobUrl = blob.Uri.ToString(),
                SizeBytes = payload.Length,
                ChunkCount = 1
            };
        }
    }
}
