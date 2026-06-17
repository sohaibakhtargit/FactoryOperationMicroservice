using FactoryOps_AccessManagementService.FactoryOpsApp.Application.DTOs;

namespace FactoryOps_AccessManagementService.FactoryOpsApp.Application.Interfaces.Repositories.TenantAdmin.Analytic_Reports
{
    public interface ITechnicianDashboardRepository
    {
        Task<TechnicianDashboardDto> GetTechnicianDashboardAsync(int tenantId,
      int userId,
      DashboardFilter filter = DashboardFilter.Month);

    }
}
