using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Repositories.TenantAdmin.TenantAdminManagement;
using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.TenantAdminManagement;

namespace FactoryOperation_AccessManagementService.FactoryOpsApp.Infrastructure.Implementation.Service.TenantAdmin.TenantAdminManagement
{
    public class ReportsAnalyticsService : IReportsAnalyticsService
    {
        private readonly IReportsAnalyticsRepository _reportsRepo;

        public ReportsAnalyticsService(IReportsAnalyticsRepository reportsRepo)
        {
            _reportsRepo = reportsRepo;
        }

        public async Task<ReportsAnalyticsDto> GetTenantAnalyticsReportAsync(int tenantId)
        {
            return await _reportsRepo.GetReportsAnalyticsData(tenantId);
        }
    }
}