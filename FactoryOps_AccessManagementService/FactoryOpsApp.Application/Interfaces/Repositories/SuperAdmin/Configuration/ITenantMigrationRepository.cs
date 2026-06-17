using FactoryOps_AccessManagementService.FactoryOpsApp.Application.Common;
using System.Threading.Tasks;

namespace FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Repositories.SuperAdmin.Configuration
{
    public interface ITenantMigrationRepository
    {
        Task<CommonResponseModel> ApplySchemaMigrationAsync();
        Task<CommonResponseModel> ApplySchemaMigrationByTenantAsync(int tenantId);
    }
}
