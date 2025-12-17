namespace FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.TenantAdminManagement
{
    public interface IReportsAnalyticsService
    {
        Task<ReportsAnalyticsDto> GetTenantAnalyticsReportAsync(int tenantId);
    }

}
