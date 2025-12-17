using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.TeamManagement;
using Microsoft.AspNetCore.Mvc;

namespace FactoryOps_AccessManagementService.Controllers.TenantAdminController.TeamManagement
{
    [Route("api/[controller]")]
    [ApiController]
    public class TeamsOverviewController : ControllerBase
    {
        private readonly ITeamsOverviewServices _teamsOverviewServices;

        public TeamsOverviewController(ITeamsOverviewServices teamsOverviewServices)
        {
            _teamsOverviewServices = teamsOverviewServices;
        }
        [HttpGet("GetTeamOverview")]
        public async Task<IActionResult> GetTeamOverviewAsync(int tenantId)
        {
            var result = await _teamsOverviewServices.GetTeamOverviewAsync(tenantId);
            return StatusCode(int.Parse(result.StatusCode), result);
        }
    }
}
