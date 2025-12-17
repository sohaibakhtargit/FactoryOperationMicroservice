using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.TenantAdminManagement;
using FactoryOpsApp.Application.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FactoryOps_AccessManagementService.Controllers.TenantAdminController.TenantAdminDashboard
{
    /// <summary>
    /// Team Management API
    /// Manages team configurations, team members, and team-based operations
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class TeamController : ControllerBase
    {
        private readonly ITeamService _teamService;

        public TeamController(ITeamService teamService)
        {
            _teamService = teamService;
        }

        /// <summary>
        /// Add team
        /// Creates new team configuration and structure
        /// </summary>
        /// <param name="dto">Team data</param>
        /// <returns>Team creation result</returns>
        /// <response code="200">Team successfully added</response>
        [Authorize(Roles = "TenantAdmin")]
        [HttpPost("Add")]
        public async Task<IActionResult> AddTeam([FromBody] AddTeamDto dto)
        {
            var result = await _teamService.AddTeam(dto);
            return Ok(result);
        }

        /// <summary>
        /// Update team
        /// Modifies existing team configuration and settings
        /// </summary>
        /// <param name="id">Team identifier</param>
        /// <param name="dto">Updated team data</param>
        /// <returns>Update operation result</returns>
        /// <response code="200">Team successfully updated</response>
        [Authorize(Roles = "TenantAdmin")]
        [HttpPost("Update/{id}")]
        public async Task<IActionResult> UpdateTeam(int id, [FromBody] AddTeamDto dto)
        {
            var result = await _teamService.UpdateTeam(id, dto);
            return Ok(result);
        }

        /// <summary>
        /// Delete team
        /// Removes team configuration from system
        /// </summary>
        /// <param name="id">Team identifier</param>
        /// <param name="TenantId">Tenant identifier</param>
        /// <returns>Deletion operation result</returns>
        /// <response code="200">Team successfully deleted</response>
        [Authorize(Roles = "TenantAdmin")]
        [HttpPost("Delete/{id}")]
        public async Task<IActionResult> DeleteTeam(int id, int TenantId)
        {
            var result = await _teamService.DeleteTeam(id, TenantId);
            return Ok(result);
        }

        /// <summary>
        /// Get all teams
        /// Retrieves complete list of all teams for tenant
        /// </summary>
        /// <param name="TenantId">Tenant identifier</param>
        /// <returns>List of all teams</returns>
        /// <response code="200">Successfully retrieved all teams</response>
        [HttpGet("Get-All")]
        public IActionResult GetAllTeams(int TenantId)
        {
            var result = _teamService.GetAllTeams(TenantId);
            return Ok(result);
        }

        [HttpGet("Get-By-Id")]
        public IActionResult GetTeamById(int TenantId, int TeamId)
        {
            var result = _teamService.GetTeamsByIdAsync(TenantId, TeamId);
            if (result == null)
                return NotFound();

            return Ok(result);
        }
    }
}