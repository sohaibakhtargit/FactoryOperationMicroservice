using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Repositories.TenantAdmin.TeamManagement;
using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.TeamManagement;
using FactoryOps_AccessManagementService.FactoryOpsApp.Application.Common;
using FactoryOpsApp.Application.DTOs;

namespace FactoryOperation_AccessManagementService.FactoryOpsApp.Infrastructure.Implementation.Service.TenantAdmin.TeamManagement
{
    public class TeamsOverviewService : ITeamsOverviewServices
    {
        private readonly ITeamsOverviewRepository _repository;
        public TeamsOverviewService(ITeamsOverviewRepository repository)
        {
            _repository = repository;
        }
        public Task<GetAllRecord<TeamOverviewDto>> GetTeamOverviewAsync(int tenantId) =>
        _repository.GetTeamOverviewAsync(tenantId);
    }
}

