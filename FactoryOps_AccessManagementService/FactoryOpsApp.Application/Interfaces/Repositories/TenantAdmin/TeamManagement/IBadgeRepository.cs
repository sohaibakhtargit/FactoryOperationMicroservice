using FactoryOps_AccessManagementService.FactoryOpsApp.Application.Common;
using FactoryOpsApp.Application.DTOs;

namespace FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Repositories.TenantAdmin.TeamManagement
{
    public interface IBadgeRepository
    {
        Task<CommonResponseModel> AddBadgeAsync(BadgeDto dto);
        Task<CommonResponseModel> UpdateBadgeAsync(BadgeDto dto);
        Task<CommonResponseModel> DeleteBadgeAsync(int badgeId, int tenantId);
        Task<GetAllRecord<GetBadgeDto>> GetAllBadgesAsync(int tenantId);
        Task<GetSpecificRecord<GetBadgeDto>> GetBadgeByIdAsync(int badgeId, int tenantId);
    }
}
