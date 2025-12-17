using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Repositories.SuperAdmin.TeamManagement;
using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Services.SuperAdmin.TeamManagement;
using FactoryOps_AccessManagementService.FactoryOpsApp.Application.Common;
using FactoryOpsApp.Application.DTOs;

namespace FactoryOperation_AccessManagementService.FactoryOpsApp.Infrastructure.Implementation.Service.SuperAdmin.TeamManagement
{
    public class SuperAdminTeamService : ISuperAdminTeamService
    {
        private readonly ISuperAdminTeamRepository _repository;

        public SuperAdminTeamService(ISuperAdminTeamRepository repository)
        {
            _repository = repository;
        }

        public async Task<GetAllRecord<SuperAdminTeamDto>> GetAllTeamsAsync()
        {
            return await _repository.GetAllTeamsFromAllTenantsAsync();
        }
    }
}
