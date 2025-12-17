using FactoryOps_AccessManagementService.FactoryOpsApp.Application.Common;

namespace FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Repositories.SuperAdmin.Configuration
{
    public interface ISuperAdminDashboardRepository
    {
        CountResponseModel GetActiveTenantsAsync();
        CountResponseModel GetActiveUsersAsync();
        Task<ReportsAnalyticsSuperAdminDto> GetTenantAnalyticsReportForSuperAdminAsync();
    }
}
