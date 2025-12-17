namespace FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Repositories.TenantAdmin.TenantAdminManagement
{
    public interface IReportsAnalyticsRepository
    {
        Task<ReportsAnalyticsDto> GetReportsAnalyticsData(int tenantId);
    }
}
