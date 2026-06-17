using FactoryOperation_WorkOrder.FactoryOpsApp.Application.DTOs;
using FactoryOperation_WorkOrder.FactoryOpsApp.Application.Interfaces.Repositories.TenantAdmin.BulkImportFileSample;
using FactoryOperation_WorkOrder.FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.BulkImportFileSample;
using FactoryOpsApp.Application.DTOs;

namespace FactoryOperation_WorkOrder.FactoryOpsApp.Infrastructure.Implementation.Services.BulkImportFileSample
{
    public class BulkImportSampleService : IBulkImportSampleService
    {
        private readonly IBulkImportSampleRepository _bulkImportSampleRepository;
        public BulkImportSampleService(IBulkImportSampleRepository bulkImportSampleRepository)
        {
            _bulkImportSampleRepository = bulkImportSampleRepository;
        }
        public async Task UploadSampleAsync(BulkImportSampleDto request)
        {
            await _bulkImportSampleRepository.UploadSampleAsync(request);
        }
        public async Task<(byte[] FileBytes, string FileName)> DownloadSampleAsync(int tenantId, string moduleName)
        {
            return await _bulkImportSampleRepository.DownloadSampleAsync(tenantId, moduleName);
        }
    }
}
