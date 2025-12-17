using FactoryOps_AccessManagementService.FactoryOpsApp.Application.Common;
using FactoryOpsApp.Application.DTOs;

namespace FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.TenantAdminManagement
{
    public interface ITeamService
    {
        Task<CommonResponseModel> AddTeam(AddTeamDto dto);
        Task<CommonResponseModel> UpdateTeam(int id, AddTeamDto dto);
        Task<CommonResponseModel> DeleteTeam(int id, int TenantId);
        GetAllRecord<GetTeamDto> GetAllTeams(int TenantId);
        GetSpecificRecord<GetTeamDto> GetTeamsByIdAsync(int tenantId, int teamId);
    }
}
