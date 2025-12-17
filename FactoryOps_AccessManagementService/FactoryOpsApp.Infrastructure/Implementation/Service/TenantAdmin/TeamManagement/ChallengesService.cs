using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Repositories.TenantAdmin.TeamManagement;
using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.TeamManagement;
using FactoryOps_AccessManagementService.FactoryOpsApp.Application.Common;
using FactoryOpsApp.Application.DTOs;

namespace FactoryOperation_AccessManagementService.FactoryOpsApp.Infrastructure.Implementation.Service.TenantAdmin.TeamManagement
{
    public class ChallengesService : IChallengesService
    {
        private readonly IChallengesRepository _challengesRepository;

        public ChallengesService(IChallengesRepository challengesRepository)
        {
            _challengesRepository = challengesRepository;
        }
        public Task<CommonResponseModel> CreateChallengesAsync(ChallengesDto dto)
            => _challengesRepository.CreateChallengesAsync(dto);
        public Task<GetAllRecord<ChallengeBoardData>> GetChallengeBoardAsync(int tenantId)
             => _challengesRepository.GetChallengeBoardAsync(tenantId);
    }
}
