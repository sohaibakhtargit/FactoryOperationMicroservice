using FactoryOps_AccessManagementService.FactoryOpsApp.Application.Common;
using FactoryOpsApp.Application.DTOs;

namespace FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.TeamManagement
{
    public interface IChallengesService
    {
        Task<CommonResponseModel> CreateChallengesAsync(ChallengesDto dto);
        Task<GetAllRecord<ChallengeBoardData>> GetChallengeBoardAsync(int tenantId);

    }
}
