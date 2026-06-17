using FactoryOperation_AuditTrailService.FactoryOpsApp.Application.Interfaces.Services.Shared;

namespace FactoryOperation_AuditTrailService.FactoryOpsApp.Infrastructure.Implementation.Shared
{
    public class LocalFilePayloadReader : IExternalPayloadReader
    {
        private readonly ILogger<LocalFilePayloadReader> _logger;

        public LocalFilePayloadReader(ILogger<LocalFilePayloadReader> logger)
        {
            _logger = logger;
        }

        public async Task<byte[]> ReadAsync(string blobUrl, CancellationToken ct)
        {
            if (!File.Exists(blobUrl))
                throw new FileNotFoundException("External payload not found", blobUrl);

            _logger.LogInformation("Reading external payload from {Path}", blobUrl);

            return await File.ReadAllBytesAsync(blobUrl, ct);
        }
    }

}
