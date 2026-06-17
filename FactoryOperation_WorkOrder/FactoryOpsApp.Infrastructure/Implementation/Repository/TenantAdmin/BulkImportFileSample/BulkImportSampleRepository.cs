using FactoryOperation_WorkOrder.FactoryOpsApp.Application.DTOs;
using FactoryOperation_WorkOrder.FactoryOpsApp.Application.Interfaces.Repositories.TenantAdmin.BulkImportFileSample;
using FactoryOperation_WorkOrder.FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.Common;
using FactoryOperation_WorkOrder.FactoryOpsApp.Domain.Entities.FactoryOpsTenants;
using FactoryOpsApp.Application.DTOs;
using FactoryOpsApp.Infrastructure.DBContext;
using Microsoft.EntityFrameworkCore;

namespace FactoryOperation_WorkOrder.FactoryOpsApp.Infrastructure.Implementation.Repository.TenantAdmin.BulkImportFileSample
{
    public class BulkImportSampleRepository : IBulkImportSampleRepository
    {
        private readonly IFileStorageService _fileStorageService;
        private readonly TenantDbContextFactory _tenantDbContext;

        public BulkImportSampleRepository(IFileStorageService fileStorageService, TenantDbContextFactory tenantDbContext)
        {
            _fileStorageService = fileStorageService;
            _tenantDbContext = tenantDbContext;
        }

        public async Task UploadSampleAsync(BulkImportSampleDto request)
        {
            using var tenantDb = _tenantDbContext.GetTenantDbContext(request.TenantId);

            var relativePath = await _fileStorageService.SaveFileAsync(
                request.File,
                "uploads/bulk-import-samples"
            );

            var entity = new BulkImportFilesSamples
            {
                TenantId = request.TenantId,
                ModuleName = request.ModuleName,
                FileName = request.File.FileName,
                FilePath = relativePath,
                CreatedBy = request.CreatedBy,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            tenantDb.BulkImportFilesSamples.Add(entity);
            await tenantDb.SaveChangesAsync();
        }

        public async Task<(byte[] FileBytes, string FileName)> DownloadSampleAsync(
            int tenantId,
            string moduleName)
        {
            using var tenantDb = _tenantDbContext.GetTenantDbContext(tenantId);

            var sample = await tenantDb.BulkImportFilesSamples
                .Where(x =>
                    x.ModuleName == moduleName &&
                    x.IsActive &&
                    (x.TenantId == tenantId || x.TenantId == null))
                .OrderByDescending(x => x.TenantId == tenantId)
                .FirstOrDefaultAsync();

            if (sample == null)
                throw new Exception("Sample file not found");

            var bytes = await _fileStorageService.GetFileBytesAsync(sample.FilePath);
            return (bytes, sample.FileName);
        }
    }
}
