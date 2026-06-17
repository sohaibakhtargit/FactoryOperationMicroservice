using FactoryOperation_WorkOrder.FactoryOpsApp.Application.DTOs;
using FactoryOpsApp.Application.DTOs;

namespace FactoryOperation_WorkOrder.FactoryOpsApp.Application.Interfaces.Repositories.TenantAdmin.BulkImportFileSample
{
    public interface IBulkImportSampleRepository
    {
        Task UploadSampleAsync(BulkImportSampleDto request);
        Task<(byte[] FileBytes, string FileName)> DownloadSampleAsync(int tenantId, string moduleName);
    }
}
