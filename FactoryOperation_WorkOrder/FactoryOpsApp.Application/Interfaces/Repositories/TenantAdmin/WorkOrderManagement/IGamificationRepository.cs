using FactoryOpsApp.Application.Common;
using FactoryOpsApp.Application.DTOs;

namespace FactoryOperation_WorkOrder.FactoryOpsApp.Application.Interfaces.Repositories.TenantAdmin.WorkOrderManagement
{
    public interface IGamificationRepository
    {
        Task<GetSpecificRecord<GamificationDashboardDto>> GetGamificationDashboardAsync(int userId, int tenantId);
        Task<GetAllRecord<AchievementDto>> GetUserAchievementsAsync(int userId, int tenantId);
        Task<GetAllRecord<ChallengeProgressDto>> GetActiveChallengesAsync(int userId, int tenantId);
        Task<GetAllRecord<LeaderboardEntryDto>> GetLeaderboardAsync(int tenantId, int topCount = 10);
        Task<GetSpecificRecord<GamificationStatsDto>> GetUserGamificationStatsAsync(int userId, int tenantId);
    }
}
