using FactoryOperation_KafkaMqttService.FactoryOpsApp.Shared.Interfaces;
using FactoryOperation_KafkaMqttService.FactoryOpsApp.Shared.Models;

namespace FactoryOperation_KafkaMqttService.FactoryOpsApp.Shared.Services
{
    public class LocalFilePayloadStore : IExternalPayloadStore
    {
        private readonly string _basePath;
        private readonly ILogger<LocalFilePayloadStore> _logger;

        public LocalFilePayloadStore(
            IWebHostEnvironment env,
            ILogger<LocalFilePayloadStore> logger)
        {
            _logger = logger;

            // Store files in wwwroot/payloads
            _basePath = Path.Combine(env.WebRootPath, "payloads");

            if (!Directory.Exists(_basePath))
            {
                Directory.CreateDirectory(_basePath);
                _logger.LogInformation("Created payload store directory: {Path}", _basePath);
            }
        }

        public async Task<ExternalPayloadReference> SaveAsync(
            byte[] payload,
            string correlationId,
            int? tenantId,
            CancellationToken ct)
        {
            var tenantFolder = Path.Combine(_basePath, (tenantId ?? 0).ToString());
            Directory.CreateDirectory(tenantFolder);

            var fileName = $"{DateTime.UtcNow:yyyyMMddHHmmss}-{correlationId}-{Guid.NewGuid():N}.json";
            var fullPath = Path.Combine(tenantFolder, fileName);

            await File.WriteAllBytesAsync(fullPath, payload, ct);

            _logger.LogInformation("External payload saved locally: {Path}", fullPath);

            return new ExternalPayloadReference
            {
                BlobUrl = fullPath,
                SizeBytes = payload.Length,
                ChunkCount = 1
            };
        }
    }
}
