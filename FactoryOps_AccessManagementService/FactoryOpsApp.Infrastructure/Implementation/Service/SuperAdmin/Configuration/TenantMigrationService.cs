using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Repositories.SuperAdmin.Configuration;
using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Services.SuperAdmin.Configuration;
using FactoryOps_AccessManagementService.FactoryOpsApp.Application.Common;

namespace FactoryOperation_AccessManagementService.FactoryOpsApp.Infrastructure.Implementation.Service.SuperAdmin.Configuration
{
    public class TenantMigrationService : ITenantMigrationService
    {
        private readonly ITenantMigrationRepository _migrationRepo;

        public TenantMigrationService(ITenantMigrationRepository migrationRepo)
        {
            _migrationRepo = migrationRepo;
        }

        public async Task<CommonResponseModel> ApplySchemaMigrationAsync()
        {
            return await _migrationRepo.ApplySchemaMigrationAsync();
        }

        public async Task<CommonResponseModel> ApplySchemaMigrationByTenantAsync(int tenantId)
        {
            return await _migrationRepo.ApplySchemaMigrationByTenantAsync(tenantId);
        }
    }
}
