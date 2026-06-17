using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Repositories.TenantAdmin.TeamManagement;
using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.ExceptionLogger;
using FactoryOps_AccessManagementService.FactoryOpsApp.Application.Common;
using FactoryOpsApp.Application.DTOs;
using FactoryOpsApp.Domain.Entities.FactoryOpsTenants;
using FactoryOpsApp.Infrastructure.DBContext;
using Microsoft.EntityFrameworkCore;

namespace FactoryOperation_AccessManagementService.FactoryOpsApp.Infrastructure.Implementation.Repository.TenantAdmin.TeamManagement
{
    public class GamificationRepository : IGamificationRepository
    {
        private readonly TenantDbContextFactory _tenantDbContext;
        private readonly IExceptionLoggerService _exceptionLogger;

        public GamificationRepository(TenantDbContextFactory tenantDbContext,
                                    IExceptionLoggerService exceptionLogger)
        {
            _tenantDbContext = tenantDbContext;
            _exceptionLogger = exceptionLogger;
        }

        public async Task<GetSpecificRecord<GamificationDashboardDto>> GetGamificationDashboardAsync(int userId, int tenantId)
        {
            try
            {
                using var tenantDb = _tenantDbContext.GetTenantDbContext(tenantId);

                
                var totalPoints = await tenantDb.PointAssignments
                    .Where(pa => pa.AssignedToUserId == userId && pa.Status == PointAssignmentStatus.Completed)
                    .SumAsync(pa => pa.TotalPoints);

                
                var level = totalPoints / 100 + 1;
                var pointsToNextLevel = level * 100 - totalPoints;

                
                var completedTasks = await tenantDb.PointAssignments
                    .CountAsync(pa => pa.AssignedToUserId == userId && pa.Status == PointAssignmentStatus.Completed);

                
                var recentAchievements = await GetRecentAchievementsAsync(userId, tenantDb);

                
                var activeChallenges = await GetActiveChallengesAsync(userId, tenantDb);

                
                var leaderboard = await GetLeaderboardDataAsync(tenantDb, 10);

                var dashboard = new GamificationDashboardDto
                {
                    TotalPoints = totalPoints,
                    Level = level,
                    PointsToNextLevel = pointsToNextLevel,
                    CompletedTasks = completedTasks,
                    OnTimeCompletionRate = await CalculateOnTimeCompletionRateAsync(userId, tenantDb),
                    CurrentStreakDays = await CalculateCurrentStreakAsync(userId, tenantDb),
                    UnlockedAchievements = recentAchievements.Count(a => a.IsEarned),
                    TotalAchievements = 20, 
                    RecentAchievements = recentAchievements,
                    ActiveChallenges = activeChallenges,
                    Leaderboard = leaderboard
                };

                return new GetSpecificRecord<GamificationDashboardDto>
                {
                    StatusCode = "200",
                    StatusMessage = "Gamification dashboard retrieved successfully",
                    Data = dashboard
                };
            }
            catch (Exception ex)
            {
                await _exceptionLogger.LogExceptionAsync(ex, "Gamification-Module", "GetGamificationDashboardAsync", tenantId, userId);
                return new GetSpecificRecord<GamificationDashboardDto>
                {
                    StatusCode = "500",
                    StatusMessage = $"Error: {ex.Message}"
                };
            }
        }

        private async Task<List<AchievementDto>> GetRecentAchievementsAsync(int userId, FactoryOpsDBContext tenantDb)
        {
            var userBadges = await tenantDb.UserBadges
                .Where(ub => ub.UserId == userId)
                .Include(ub => ub.Badge)
                .OrderByDescending(ub => ub.AwardedDate)
                .Take(5)
                .ToListAsync();

            var achievements = userBadges.Select(ub => new AchievementDto
            {
                BadgeName = ub.Badge?.Name ?? "Unknown Badge",
                Description = ub.Badge?.Description ?? "",
                Points = ub.Badge?.PointsRequired ?? 0,
                EarnedDate = ub.AwardedDate,
                IsEarned = ub.IsAwarded
            }).ToList();

            
            if (achievements.Count < 5)
            {
                var availableBadges = await tenantDb.Badges
                    .Where(b => !userBadges.Select(ub => ub.BadgeId).Contains(b.BadgeId))
                    .Take(5 - achievements.Count)
                    .ToListAsync();

                achievements.AddRange(availableBadges.Select(b => new AchievementDto
                {
                    BadgeName = b.Name,
                    Description = b.Description,
                    Points = b.PointsRequired,
                    IsEarned = false
                }));
            }

            return achievements;
        }

        private async Task<List<ChallengeProgressDto>> GetActiveChallengesAsync(int userId, FactoryOpsDBContext tenantDb)
        {
            
            // For now, using integer 1 for active status as in your original code
            var activeChallenges = await tenantDb.Challenges
                .Where(c => c.IsActive &&
                            c.StartDate <= DateTime.UtcNow &&
                            c.EndDate >= DateTime.UtcNow)
                .Take(3)
                .ToListAsync();

            return activeChallenges.Select(c => new ChallengeProgressDto
            {
                Title = c.Title,
                Description = c.Description,
                CurrentProgress = CalculateChallengeProgress(c, userId),
                Target = 100, 
                Reward = c.Reward
            }).ToList();
        }

        private int CalculateChallengeProgress(Challenge challenge, int userId)
        {
            
            if (challenge.ParticipantIds != null && challenge.ParticipantIds.Contains(userId))
            {
                return 50;
            }
            return 0;
        }

        private async Task<List<LeaderboardEntryDto>> GetLeaderboardDataAsync(FactoryOpsDBContext tenantDb, int topCount)
        {
            var leaderboardData = await tenantDb.PointAssignments
                .Where(pa => pa.Status == PointAssignmentStatus.Completed) 
                .GroupBy(pa => pa.AssignedToUserId)
                .Select(g => new
                {
                    UserId = g.Key,
                    TotalPoints = g.Sum(pa => pa.TotalPoints),
                    TasksCompleted = g.Count()
                })
                .OrderByDescending(x => x.TotalPoints)
                .Take(topCount)
                .ToListAsync();

            var leaderboard = new List<LeaderboardEntryDto>();
            int rank = 1;

            foreach (var item in leaderboardData)
            {
                var user = await tenantDb.FactoryUsers
                    .FirstOrDefaultAsync(u => u.UserId == item.UserId);

                leaderboard.Add(new LeaderboardEntryDto
                {
                    UserName = user != null ? $"{user.FirstName} {user.LastName}" : "Unknown User",
                    TasksCompleted = item.TasksCompleted,
                    TotalPoints = item.TotalPoints,
                    Rank = rank++
                });
            }

            return leaderboard;
        }

        private async Task<decimal> CalculateOnTimeCompletionRateAsync(int userId, FactoryOpsDBContext tenantDb)
        {
            var completedTasks = await tenantDb.PointAssignments
                .Where(pa => pa.AssignedToUserId == userId &&
                           pa.Status == PointAssignmentStatus.Completed && 
                           pa.CompletionDate.HasValue)
                .ToListAsync();

            if (!completedTasks.Any()) return 0;

            var onTimeTasks = completedTasks.Count(pa =>
                pa.CreatedAt.HasValue &&
                pa.CompletionDate.HasValue &&
                pa.CompletionDate.Value <= pa.CreatedAt.Value.AddDays(1));

            return Math.Round((decimal)onTimeTasks / completedTasks.Count * 100, 2);
        }

        private async Task<int> CalculateCurrentStreakAsync(int userId, FactoryOpsDBContext tenantDb)
        {
            var recentCompletions = await tenantDb.PointAssignments
                .Where(pa => pa.AssignedToUserId == userId &&
                             pa.Status == PointAssignmentStatus.Completed &&
                             pa.CompletionDate.HasValue)
                .OrderByDescending(pa => pa.CompletionDate)
                .Select(pa => pa.CompletionDate.Value.Date)
                .Distinct()
                .ToListAsync();

            if (!recentCompletions.Any()) return 0;

            int streak = 1;
            var today = DateTime.UtcNow.Date;

            if ((today - recentCompletions.First()).TotalDays > 1)
                return 0;

            for (int i = 0; i < recentCompletions.Count - 1; i++)
            {
                var diff = (recentCompletions[i] - recentCompletions[i + 1]).TotalDays;
                if (diff == 1)
                    streak++;
                else
                    break;
            }

            return streak;
        }

        public async Task<GetAllRecord<AchievementDto>> GetUserAchievementsAsync(int userId, int tenantId)
        {
            try
            {
                using var tenantDb = _tenantDbContext.GetTenantDbContext(tenantId);

                var userBadges = await tenantDb.UserBadges
                    .Where(ub => ub.UserId == userId)
                    .Include(ub => ub.Badge)
                    .OrderByDescending(ub => ub.AwardedDate)
                    .ToListAsync();

                var allBadges = await tenantDb.Badges.ToListAsync();

                var achievements = allBadges.Select(badge => new AchievementDto
                {
                    BadgeName = badge.Name,
                    Description = badge.Description,
                    Points = badge.PointsRequired,
                    EarnedDate = userBadges.FirstOrDefault(ub => ub.BadgeId == badge.BadgeId)?.AwardedDate,
                    IsEarned = userBadges.Any(ub => ub.BadgeId == badge.BadgeId && ub.IsAwarded)
                }).ToList();

                return new GetAllRecord<AchievementDto>
                {
                    StatusCode = "200",
                    StatusMessage = "User achievements retrieved successfully",
                    GetAllData = achievements
                };
            }
            catch (Exception ex)
            {
                await _exceptionLogger.LogExceptionAsync(ex, "Gamification-Module", "GetUserAchievementsAsync", tenantId, userId);
                return new GetAllRecord<AchievementDto>
                {
                    StatusCode = "500",
                    StatusMessage = $"Error: {ex.Message}"
                };
            }
        }

        public async Task<GetAllRecord<ChallengeProgressDto>> GetActiveChallengesAsync(int userId, int tenantId)
        {
            try
            {
                using var tenantDb = _tenantDbContext.GetTenantDbContext(tenantId);

                
                var activeChallenges = await tenantDb.Challenges
                    .Where(c => c.IsActive &&
                               c.StartDate <= DateTime.UtcNow &&
                               c.EndDate >= DateTime.UtcNow)
                    .ToListAsync();

                var challenges = activeChallenges.Select(c => new ChallengeProgressDto
                {
                    Title = c.Title,
                    Description = c.Description,
                    CurrentProgress = CalculateChallengeProgress(c, userId),
                    Target = 100,
                    Reward = c.Reward
                }).ToList();

                return new GetAllRecord<ChallengeProgressDto>
                {
                    StatusCode = "200",
                    StatusMessage = "Active challenges retrieved successfully",
                    GetAllData = challenges
                };
            }
            catch (Exception ex)
            {
                await _exceptionLogger.LogExceptionAsync(ex, "Gamification-Module", "GetActiveChallengesAsync", tenantId, userId);
                return new GetAllRecord<ChallengeProgressDto>
                {
                    StatusCode = "500",
                    StatusMessage = $"Error: {ex.Message}"
                };
            }
        }

        public async Task<GetAllRecord<LeaderboardEntryDto>> GetLeaderboardAsync(int tenantId, int topCount = 10)
        {
            try
            {
                using var tenantDb = _tenantDbContext.GetTenantDbContext(tenantId);

                var leaderboard = await GetLeaderboardDataAsync(tenantDb, topCount);

                return new GetAllRecord<LeaderboardEntryDto>
                {
                    StatusCode = "200",
                    StatusMessage = "Leaderboard retrieved successfully",
                    GetAllData = leaderboard
                };
            }
            catch (Exception ex)
            {
                await _exceptionLogger.LogExceptionAsync(ex, "Gamification-Module", "GetLeaderboardAsync", tenantId, null);
                return new GetAllRecord<LeaderboardEntryDto>
                {
                    StatusCode = "500",
                    StatusMessage = $"Error: {ex.Message}"
                };
            }
        }

        public async Task<GetSpecificRecord<GamificationStatsDto>> GetUserGamificationStatsAsync(int userId, int tenantId)
        {
            try
            {
                using var tenantDb = _tenantDbContext.GetTenantDbContext(tenantId);

                var totalPoints = await tenantDb.PointAssignments
                    .Where(pa => pa.AssignedToUserId == userId && pa.Status == PointAssignmentStatus.Completed) 
                    .SumAsync(pa => pa.TotalPoints);

                var level = totalPoints / 100 + 1;

                var currentMonth = DateTime.UtcNow.Month;
                var currentYear = DateTime.UtcNow.Year;

                var tasksThisMonth = await tenantDb.PointAssignments
                    .CountAsync(pa => pa.AssignedToUserId == userId &&
                                      pa.Status == PointAssignmentStatus.Completed && 
                                      pa.CompletionDate.HasValue &&
                                      pa.CompletionDate.Value.Month == currentMonth &&
                                      pa.CompletionDate.Value.Year == currentYear);

                var streak = await CalculateCurrentStreakAsync(userId, tenantDb);

                var stats = new GamificationStatsDto
                {
                    TotalPoints = totalPoints,
                    CurrentLevel = level,
                    TasksCompletedThisMonth = tasksThisMonth,
                    AverageCompletionTime = 85.5m,
                    StreakDays = streak,
                    Rank = await CalculateUserRankAsync(userId, tenantDb),
                    TotalUsers = await tenantDb.FactoryUsers.CountAsync(u => u.IsActive && !u.IsDeleted)
                };

                return new GetSpecificRecord<GamificationStatsDto>
                {
                    StatusCode = "200",
                    StatusMessage = "User gamification stats retrieved successfully",
                    Data = stats
                };
            }
            catch (Exception ex)
            {
                await _exceptionLogger.LogExceptionAsync(ex, "Gamification-Module", "GetUserGamificationStatsAsync", tenantId, userId);
                return new GetSpecificRecord<GamificationStatsDto>
                {
                    StatusCode = "500",
                    StatusMessage = $"Error: {ex.Message}"
                };
            }
        }

        private async Task<int> CalculateUserRankAsync(int userId, FactoryOpsDBContext tenantDb)
        {
            var userPoints = await tenantDb.PointAssignments
                .Where(pa => pa.AssignedToUserId == userId && pa.Status == PointAssignmentStatus.Completed) 
                .SumAsync(pa => pa.TotalPoints);

            var usersWithMorePoints = await tenantDb.PointAssignments
                .Where(pa => pa.Status == PointAssignmentStatus.Completed) 
                .GroupBy(pa => pa.AssignedToUserId)
                .Where(g => g.Sum(pa => pa.TotalPoints) > userPoints)
                .CountAsync();

            return usersWithMorePoints + 1;
        }
    }
}