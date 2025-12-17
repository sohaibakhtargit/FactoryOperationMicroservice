using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.TeamManagement;
using FactoryOpsApp.Application.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FactoryOps_AccessManagementService.Controllers.TenantAdminController.TeamManagement
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChallengesController : ControllerBase
    {
        private readonly IChallengesService _challengesService;

        public ChallengesController(IChallengesService challengesService)
        {
            _challengesService = challengesService;
        }
        [HttpPost("Add-Challenges")]
        public async Task<IActionResult> CreateChallengesAsync([FromBody] ChallengesDto dto)
        {
            var result = await _challengesService.CreateChallengesAsync(dto);
            return StatusCode(int.Parse(result.StatusCode), result);
        }
        [HttpGet("Get-Challenges")]
        public async Task<IActionResult> GetChallengeBoardAsync(int tenantId)
        {
            var result = await _challengesService.GetChallengeBoardAsync(tenantId);
            return StatusCode(int.Parse(result.StatusCode), result);
        }
    }

}
