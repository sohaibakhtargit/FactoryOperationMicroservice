using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Repositories.TenantAdmin.TeamManagement;
using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.TeamManagement;
using FactoryOps_AccessManagementService.FactoryOpsApp.Application.Common;
using FactoryOpsApp.Application.DTOs;

namespace FactoryOperation_AccessManagementService.FactoryOpsApp.Infrastructure.Implementation.Service.TenantAdmin.TeamManagement
{
    public class GamificationService : IGamificationService
    {
        private readonly IGamificationRepository _repository;

        public GamificationService(IGamificationRepository repository)
        {
            _repository = repository;
        }

        public Task<GetSpecificRecord<GamificationDashboardDto>> GetGamificationDashboardAsync(int userId, int tenantId)
        {
            return _repository.GetGamificationDashboardAsync(userId, tenantId);
        }

        public Task<GetAllRecord<AchievementDto>> GetUserAchievementsAsync(int userId, int tenantId)
        {
            return _repository.GetUserAchievementsAsync(userId, tenantId);
        }

        public Task<GetAllRecord<ChallengeProgressDto>> GetActiveChallengesAsync(int userId, int tenantId)
        {
            return _repository.GetActiveChallengesAsync(userId, tenantId);
        }

        public Task<GetAllRecord<LeaderboardEntryDto>> GetLeaderboardAsync(int tenantId, int topCount = 10)
        {
            return _repository.GetLeaderboardAsync(tenantId, topCount);
        }

        public Task<GetSpecificRecord<GamificationStatsDto>> GetUserGamificationStatsAsync(int userId, int tenantId)
        {
            return _repository.GetUserGamificationStatsAsync(userId, tenantId);
        }
    }
}