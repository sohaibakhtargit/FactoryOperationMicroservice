using FactoryOps_AccessManagementService.FactoryOpsApp.Application.Common;
using FactoryOpsApp.Application.DTOs;

namespace FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Repositories.TenantAdmin.TeamManagement
{
    public interface IChallengesRepository
    {
        Task<CommonResponseModel> CreateChallengesAsync(ChallengesDto dto);
        Task<GetAllRecord<ChallengeBoardData>> GetChallengeBoardAsync(int tenantId);
    }
}
