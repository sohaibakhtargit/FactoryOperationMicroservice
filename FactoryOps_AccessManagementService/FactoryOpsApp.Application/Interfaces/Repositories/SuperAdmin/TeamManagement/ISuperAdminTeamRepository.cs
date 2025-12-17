using FactoryOps_AccessManagementService.FactoryOpsApp.Application.Common;
using FactoryOpsApp.Application.DTOs;

namespace FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Repositories.SuperAdmin.TeamManagement
{
    public interface ISuperAdminTeamRepository
    {
        Task<GetAllRecord<SuperAdminTeamDto>> GetAllTeamsFromAllTenantsAsync();
    }
}
