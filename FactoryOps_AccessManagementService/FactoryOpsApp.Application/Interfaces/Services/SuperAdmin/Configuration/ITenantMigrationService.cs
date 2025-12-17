using FactoryOps_AccessManagementService.FactoryOpsApp.Application.Common;

namespace FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Services.SuperAdmin.Configuration
{
    public interface ITenantMigrationService
    {
        Task<CommonResponseModel> ApplySchemaMigrationAsync();
        Task<CommonResponseModel> ApplySchemaMigrationByTenantAsync(int tenantId);
    }
}
