using FactoryOps_AccessManagementService.FactoryOpsApp.Application.Common;
using FactoryOpsApp.Application.DTOs;

namespace FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Services.SuperAdmin.TeamManagement
{
    public interface ISuperAdminTeamService
    {
        Task<GetAllRecord<SuperAdminTeamDto>> GetAllTeamsAsync();
    }
}
