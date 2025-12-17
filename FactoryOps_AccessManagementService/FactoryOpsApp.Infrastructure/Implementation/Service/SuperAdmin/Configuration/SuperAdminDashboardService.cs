using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Repositories.SuperAdmin.Configuration;
using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Services.SuperAdmin.Configuration;
using FactoryOps_AccessManagementService.FactoryOpsApp.Application.Common;

namespace FactoryOperation_AccessManagementService.FactoryOpsApp.Infrastructure.Implementation.Service.SuperAdmin.Configuration
{
    public class SuperAdminDashboardService : ISuperAdminDashboardService
    {
        private readonly ISuperAdminDashboardRepository _tenantRepository;

        public SuperAdminDashboardService(ISuperAdminDashboardRepository tenantRepository)
        {
            _tenantRepository = tenantRepository;
        }
        public CountResponseModel GetActiveTenantsAsync()
        {
            return _tenantRepository.GetActiveTenantsAsync();
        }

        public CountResponseModel GetActiveUsersAsync()
        {
            return _tenantRepository.GetActiveUsersAsync();
        }
        public async Task<ReportsAnalyticsSuperAdminDto> GetTenantAnalyticsReportForSuperAdminAsync()
        {
            return await _tenantRepository.GetTenantAnalyticsReportForSuperAdminAsync();
        }
    }
}
