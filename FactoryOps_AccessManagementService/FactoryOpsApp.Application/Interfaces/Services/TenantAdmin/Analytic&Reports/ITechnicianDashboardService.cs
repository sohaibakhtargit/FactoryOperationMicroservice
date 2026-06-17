using FactoryOps_AccessManagementService.FactoryOpsApp.Application.Common;
using FactoryOps_AccessManagementService.FactoryOpsApp.Application.DTOs;

namespace FactoryOps_AccessManagementService.FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.Analytic_Reports
{
    public interface ITechnicianDashboardService
    {
        Task<GetSpecificRecord<TechnicianDashboardDto>> GetTechnicianDashboardAsync(int tenantId, int userId, DashboardFilter filter = DashboardFilter.Month);
    }
}
