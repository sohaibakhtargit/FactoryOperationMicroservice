using FactoryOps_AccessManagementService.FactoryOpsApp.Application.Common;
using FactoryOpsApp.Application.DTOs;

namespace FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.TeamManagement
{
    public interface IUserBadgeService
    {
        Task<CommonResponseModel> AddUserBadgeAsync(UserBadgeDto dto);
        Task<CommonResponseModel> AwardUserBadgeAsync(AwardUserBadgeDto dto);
        Task<CommonResponseModel> UpdateUserBadgeProgressAsync(UpdateUserBadgeProgressDto dto);
        Task<CommonResponseModel> DeleteUserBadgeAsync(int userBadgeId, int tenantId);

        Task<GetAllRecord<GetUserBadgeDto>> GetAllUserBadgesAsync(int tenantId);
        Task<GetSpecificRecord<GetUserBadgeDto>> GetUserBadgeByIdAsync(int userBadgeId, int tenantId);
        Task<GetAllRecord<GetUserBadgeDto>> GetUserBadgesByUserAsync(int userId, int tenantId);
        Task<GetAllRecord<GetUserBadgeDto>> GetUserBadgesByBadgeAsync(int badgeId, int tenantId);
    }
}
