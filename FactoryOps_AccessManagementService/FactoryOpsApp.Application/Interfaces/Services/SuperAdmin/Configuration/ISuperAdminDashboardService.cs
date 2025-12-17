
using FactoryOps_AccessManagementService.FactoryOpsApp.Application.Common;

namespace FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Services.SuperAdmin.Configuration
{
    public interface ISuperAdminDashboardService
    {
        CountResponseModel GetActiveTenantsAsync();
        CountResponseModel GetActiveUsersAsync();
        Task<ReportsAnalyticsSuperAdminDto> GetTenantAnalyticsReportForSuperAdminAsync();
    }
}
