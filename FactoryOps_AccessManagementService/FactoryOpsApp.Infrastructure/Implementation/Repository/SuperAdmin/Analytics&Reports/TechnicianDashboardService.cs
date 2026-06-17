using FactoryOps_AccessManagementService.FactoryOpsApp.Application.Common;
using FactoryOps_AccessManagementService.FactoryOpsApp.Application.DTOs;
using FactoryOps_AccessManagementService.FactoryOpsApp.Application.Interfaces.Repositories.TenantAdmin.Analytic_Reports;
using FactoryOps_AccessManagementService.FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.Analytic_Reports;
using static FactoryOps_AccessManagementService.FactoryOpsApp.Common.CommonConstant;

namespace FactoryOps_AccessManagementService.FactoryOpsApp.Infrastructure.Implementation.Repository.SuperAdmin.Analytics_Reports
{
    public class TechnicianDashboardService : ITechnicianDashboardService
    {
        private readonly ITechnicianDashboardRepository _repository;

        public TechnicianDashboardService(ITechnicianDashboardRepository repository)
        {
            _repository = repository;
        }

        public async Task<GetSpecificRecord<TechnicianDashboardDto>> GetTechnicianDashboardAsync(int tenantId, int userId, DashboardFilter filter = DashboardFilter.Month)
        {
            var response = new GetSpecificRecord<TechnicianDashboardDto>();

            var data = await _repository.GetTechnicianDashboardAsync(tenantId, userId, filter);

            response.StatusCode = StatusCode.Success;
            response.StatusMessage = "Technician dashboard fetched successfully";
            response.Data = data;

            return response;
        }
    }
}
