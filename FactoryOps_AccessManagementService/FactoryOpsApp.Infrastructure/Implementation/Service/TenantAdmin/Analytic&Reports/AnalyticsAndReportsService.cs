using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Repositories.TenantAdmin.Analytic_Reports;
using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.Analytic_Reports;
using FactoryOps_AccessManagementService.FactoryOpsApp.Application.Common;
using FactoryOpsApp.Application.DTOs;

namespace FactoryOperation_AccessManagementService.FactoryOpsApp.Infrastructure.Implementation.Service.TenantAdmin.Analytic_Reports
{
    public class AnalyticsAndReportsService : IAnalyticsAndReportsServices
    {
        IAnalyticsAndReportsRepository _repository;

        public AnalyticsAndReportsService(IAnalyticsAndReportsRepository repository)
        {
            _repository = repository;
        }
        public Task<GetAllRecord<AnalyticsAndReportsDto>> GetAnalyticsAndReportsAsync(
           int tenantId, DateTime startDate, DateTime endDate, CategoryType category = CategoryType.All)
            => _repository.GetAnalyticsAndReportsAsync(tenantId, startDate, endDate, category);
        public Task<byte[]> ExportAnalyticsAndReportsAsync(
            int tenantId, DateTime startDate, DateTime endDate, CategoryType category = CategoryType.All)
            => _repository.ExportAnalyticsAndReportsAsync(tenantId, startDate, endDate, category);
    }
}
