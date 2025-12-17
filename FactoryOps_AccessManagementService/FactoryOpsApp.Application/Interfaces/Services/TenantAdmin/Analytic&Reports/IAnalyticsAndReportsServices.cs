using FactoryOps_AccessManagementService.FactoryOpsApp.Application.Common;
using FactoryOpsApp.Application.DTOs;

namespace FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.Analytic_Reports
{
    public interface IAnalyticsAndReportsServices
    {
        Task<GetAllRecord<AnalyticsAndReportsDto>> GetAnalyticsAndReportsAsync(
           int tenantId, DateTime startDate, DateTime endDate, CategoryType category = CategoryType.All);
        Task<byte[]> ExportAnalyticsAndReportsAsync(
          int tenantId, DateTime startDate, DateTime endDate, CategoryType category = CategoryType.All);
    }
}
