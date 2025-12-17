using FactoryOps_AccessManagementService.FactoryOpsApp.Application.Common;
using FactoryOpsApp.Application.DTOs;

namespace FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Repositories.TenantAdmin.TeamManagement
{
    public interface IGamificationRepository
    {
        Task<GetSpecificRecord<GamificationDashboardDto>> GetGamificationDashboardAsync(int userId, int tenantId);
        Task<GetAllRecord<AchievementDto>> GetUserAchievementsAsync(int userId, int tenantId);
        Task<GetAllRecord<ChallengeProgressDto>> GetActiveChallengesAsync(int userId, int tenantId);
        Task<GetAllRecord<LeaderboardEntryDto>> GetLeaderboardAsync(int tenantId, int topCount = 10);
        Task<GetSpecificRecord<GamificationStatsDto>> GetUserGamificationStatsAsync(int userId, int tenantId);
    }

    public class GamificationStatsDto
    {
        public int TotalPoints { get; set; }
        public int CurrentLevel { get; set; }
        public int TasksCompletedThisMonth { get; set; }
        public decimal AverageCompletionTime { get; set; }
        public int StreakDays { get; set; }
        public int Rank { get; set; }
        public int TotalUsers { get; set; }
    }
}
