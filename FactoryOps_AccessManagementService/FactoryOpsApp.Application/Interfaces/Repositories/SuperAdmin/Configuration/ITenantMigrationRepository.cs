using FactoryOps_AccessManagementService.FactoryOpsApp.Application.Common;

namespace FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Repositories.SuperAdmin.Configuration
{
    public interface ITenantMigrationRepository
    {
        Task<CommonResponseModel> ApplySchemaMigrationAsync();
        Task<CommonResponseModel> ApplySchemaMigrationByTenantAsync(int tenantId);
    }
}
