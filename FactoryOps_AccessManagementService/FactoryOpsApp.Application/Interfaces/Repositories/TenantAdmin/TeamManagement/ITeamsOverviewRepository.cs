using FactoryOps_AccessManagementService.FactoryOpsApp.Application.Common;
using FactoryOpsApp.Application.DTOs;

namespace FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Repositories.TenantAdmin.TeamManagement
{
    public interface ITeamsOverviewRepository
    {
        Task<GetAllRecord<TeamOverviewDto>> GetTeamOverviewAsync(int tenantId);
    }
}
