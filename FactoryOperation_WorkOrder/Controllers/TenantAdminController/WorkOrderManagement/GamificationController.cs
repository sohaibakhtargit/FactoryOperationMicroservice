using FactoryOperation_WorkOrder.FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.WorkOrderServices;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FactoryOperation_WorkOrder.Controllers.TenantAdminController.WorkOrderManagement
{
    /// <summary>
    /// Gamification API
    /// Manages gamification features, user achievements, and engagement tracking
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class GamificationController : ControllerBase
    {
        private readonly IGamificationService _gamificationService;

        public GamificationController(IGamificationService gamificationService)
        {
            _gamificationService = gamificationService;
        }

        /// <summary>
        /// Get gamification dashboard
        /// Retrieves comprehensive gamification dashboard for user
        /// </summary>
        /// <param name="tenantId">Tenant identifier</param>
        /// <param name="userId">User identifier</param>
        /// <returns>Gamification dashboard data</returns>
        /// <response code="200">Successfully retrieved gamification dashboard</response>
        [HttpGet("Get-Dashboard")]
        public async Task<IActionResult> GetGamificationDashboardAsync(int tenantId, int userId)
        {
            var result = await _gamificationService.GetGamificationDashboardAsync(userId, tenantId);
            return Ok(result);
        }

        /// <summary>
        /// Get user achievements
        /// Retrieves user achievements and accomplishment history
        /// </summary>
        /// <param name="tenantId">Tenant identifier</param>
        /// <param name="userId">User identifier</param>
        /// <returns>User achievements data</returns>
        /// <response code="200">Successfully retrieved user achievements</response>
        [HttpGet("Get-UserAchievements")]
        public async Task<IActionResult> GetUserAchievementsAsync(int tenantId, int userId)
        {
            var result = await _gamificationService.GetUserAchievementsAsync(userId, tenantId);
            return Ok(result);
        }

        /// <summary>
        /// Get active challenges
        /// Retrieves currently active challenges for user participation
        /// </summary>
        /// <param name="tenantId">Tenant identifier</param>
        /// <param name="userId">User identifier</param>
        /// <returns>Active challenges data</returns>
        /// <response code="200">Successfully retrieved active challenges</response>
        [HttpGet("Get-ActiveChallenges")]
        public async Task<IActionResult> GetActiveChallengesAsync(int tenantId, int userId)
        {
            var result = await _gamificationService.GetActiveChallengesAsync(userId, tenantId);
            return Ok(result);
        }

        /// <summary>
        /// Get leaderboard
        /// Retrieves user ranking and leaderboard standings
        /// </summary>
        /// <param name="tenantId">Tenant identifier</param>
        /// <param name="topCount">Number of top users to retrieve</param>
        /// <returns>Leaderboard ranking data</returns>
        /// <response code="200">Successfully retrieved leaderboard</response>
        [HttpGet("Get-Leaderboard")]
        public async Task<IActionResult> GetLeaderboardAsync(int tenantId, int topCount = 10)
        {
            var result = await _gamificationService.GetLeaderboardAsync(tenantId, topCount);
            return Ok(result);
        }

        /// <summary>
        /// Get user statistics
        /// Retrieves user gamification statistics and performance metrics
        /// </summary>
        /// <param name="tenantId">Tenant identifier</param>
        /// <param name="userId">User identifier</param>
        /// <returns>User gamification statistics</returns>
        /// <response code="200">Successfully retrieved user statistics</response>
        [HttpGet("Get-UserStats")]
        public async Task<IActionResult> GetUserGamificationStatsAsync(int tenantId, int userId)
        {
            var result = await _gamificationService.GetUserGamificationStatsAsync(userId, tenantId);
            return Ok(result);
        }
    }
}
