using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Services.SuperAdmin.TeamManagement;
using Microsoft.AspNetCore.Mvc;

namespace FactoryOps_AccessManagementService.Controllers.SuperAdminController.TeamManagement
{
    /// <summary>
    /// Super Admin Team Management API
    /// Manages super admin team members and their access permissions
    /// </summary>
    [Route("api/superadmin/[controller]")]
    [ApiController]
    public class SuperAdminTeamController : ControllerBase
    {
        private readonly ISuperAdminTeamService _teamService;

        public SuperAdminTeamController(ISuperAdminTeamService teamService)
        {
            _teamService = teamService;
        }

        /// <summary>
        /// Get all super admin teams
        /// Retrieves complete list of all super admin teams and their members
        /// </summary>
        /// <returns>List of all super admin teams</returns>
        /// <response code="200">Successfully retrieved all super admin teams</response>
        [HttpGet("all")]
        public async Task<IActionResult> GetAllTeams()
        {
            var result = await _teamService.GetAllTeamsAsync();
            return Ok(result);
        }
    }
}