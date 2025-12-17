using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FactoryOpsApp.Application.DTOs
{
    public class GamificationDashboardDto
    {
        public int TotalPoints { get; set; }
        public int Level { get; set; }
        public int PointsToNextLevel { get; set; }
        public int CompletedTasks { get; set; }
        public decimal OnTimeCompletionRate { get; set; }
        public int CurrentStreakDays { get; set; }
        public int UnlockedAchievements { get; set; }
        public int TotalAchievements { get; set; }

        public List<AchievementDto> RecentAchievements { get; set; } = new();
        public List<ChallengeProgressDto> ActiveChallenges { get; set; } = new();
        public List<LeaderboardEntryDto> Leaderboard { get; set; } = new();
    }

    public class AchievementDto
    {
        public string BadgeName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int Points { get; set; }
        public DateTime? EarnedDate { get; set; }
        public bool IsEarned { get; set; }
    }

    public class ChallengeProgressDto
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int CurrentProgress { get; set; }
        public int Target { get; set; }
        public string Reward { get; set; } = string.Empty;
    }

    public class LeaderboardEntryDto
    {
        public string UserName { get; set; } = string.Empty;
        public int TasksCompleted { get; set; }
        public int TotalPoints { get; set; }
        public int Rank { get; set; }
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
