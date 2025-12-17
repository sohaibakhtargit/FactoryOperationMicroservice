using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Repositories.TenantAdmin.TenantAdminManagement;
using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.TenantAdminManagement;
using FactoryOps_AccessManagementService.FactoryOpsApp.Application.Common;
using FactoryOpsApp.Application.DTOs;

namespace FactoryOperation_AccessManagementService.FactoryOpsApp.Infrastructure.Implementation.Service.TenantAdmin.TenantAdminManagement
{
    public class TeamService : ITeamService
    {
        private readonly ITeamRepository _repository;

        public TeamService(ITeamRepository repository)
        {
            _repository = repository;
        }

        public Task<CommonResponseModel> AddTeam(AddTeamDto dto) => _repository.AddTeam(dto);

        public Task<CommonResponseModel> UpdateTeam(int id, AddTeamDto dto) => _repository.UpdateTeam(id, dto);

        public Task<CommonResponseModel> DeleteTeam(int id, int TenantId) => _repository.DeleteTeam(id, TenantId);

        public GetAllRecord<GetTeamDto> GetAllTeams(int TenantId) => _repository.GetAllTeams(TenantId);
        public GetSpecificRecord<GetTeamDto> GetTeamsByIdAsync(int TenantId, int TeamId) => _repository.GetTeamsByIdAsync(TenantId, TeamId);
    }
}
