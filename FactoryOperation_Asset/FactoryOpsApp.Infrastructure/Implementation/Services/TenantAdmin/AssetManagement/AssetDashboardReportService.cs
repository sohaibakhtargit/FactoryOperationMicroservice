using FactoryOpsApp.Application.Common;
using FactoryOpsApp.Application.DTOs;
using FactoryOpsApp.Application.Interfaces.Repositories.TenantAdmin.AssetManagement;
using FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.AssetManagement;

namespace FactoryOpsApp.Infrastructure.Service.TenantAdmin.AssetManagement
{
    public class AssetDashboardReportService:IAssetDashboardReportService
    {
        private readonly IAssetDashboardReportRepository _repository;
        public AssetDashboardReportService(IAssetDashboardReportRepository repository)
        {
            _repository = repository;
        }

        public Task<GetAllRecord<DashboardSummaryDto>> GetDashboardSummaryAsync(int tenantId)
            => _repository.GetDashboardSummaryAsync(tenantId);

        public Task<GetAllRecord<DashboardDataDto>> FetchDashboardDataAsync(int tenantId)
            => _repository.FetchDashboardDataAsync(tenantId);
    }
}
