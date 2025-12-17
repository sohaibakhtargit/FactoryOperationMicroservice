using FactoryOps_AccessManagementService.FactoryOpsApp.Application.Common;
using FactoryOpsApp.Application.DTOs;

namespace FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.TeamManagement
{
    public interface ITeamsOverviewServices
    {
        Task<GetAllRecord<TeamOverviewDto>> GetTeamOverviewAsync(int tenantId);
    }
}
