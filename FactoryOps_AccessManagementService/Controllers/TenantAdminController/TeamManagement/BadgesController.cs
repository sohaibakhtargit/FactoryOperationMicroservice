using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.TeamManagement;
using FactoryOps_AccessManagementService.FactoryOpsApp.Application.Common;
using FactoryOpsApp.Application.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FactoryOps_AccessManagementService.Controllers.TenantAdminController.TeamManagement
{
    /// <summary>
    /// Badge Management API
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class BadgesController : ControllerBase
    {
        private readonly IBadgeService _badgeService;

        public BadgesController(IBadgeService badgeService)
        {
            _badgeService = badgeService;
        }

        /// <summary>
        /// Create a new badge
        /// </summary>
        /// <param name="dto">Badge creation data</param>
        /// <returns>Creation status</returns>
        [HttpPost("CreateBadge")]
        public async Task<ActionResult<CommonResponseModel>> CreateBadge([FromBody] BadgeDto dto)
        {
            var result = await _badgeService.AddBadgeAsync(dto);
            return result.StatusCode == "200" ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Update an existing badge
        /// </summary>
        /// <param name="dto">Badge update data</param>
        /// <returns>Update status</returns>
        [HttpPost("UpdateBadge")]
        public async Task<ActionResult<CommonResponseModel>> UpdateBadge([FromBody] BadgeDto dto)
        {
            var result = await _badgeService.UpdateBadgeAsync(dto);
            return result.StatusCode == "200" ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Delete a badge
        /// </summary>
        /// <param name="id">Badge ID</param>
        /// <param name="tenantId">Tenant ID</param>
        /// <returns>Deletion status</returns>
        [HttpPost("DeleteBadge")]
        public async Task<ActionResult<CommonResponseModel>> DeleteBadge(int id, int tenantId)
        {
            var result = await _badgeService.DeleteBadgeAsync(id, tenantId);
            return result.StatusCode == "200" ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Get all badges for a tenant
        /// </summary>
        /// <param name="tenantId">Tenant ID</param>
        /// <returns>List of badges</returns>
        [HttpGet("GetBadges{tenantId}")]
        public async Task<ActionResult<GetAllRecord<GetBadgeDto>>> GetBadges(int tenantId)
        {
            var result = await _badgeService.GetAllBadgesAsync(tenantId);
            return result.StatusCode == "200" ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Get a specific badge by ID
        /// </summary>
        /// <param name="tenantId">Tenant ID</param>
        /// <param name="id">Badge ID</param>
        /// <returns>Badge details</returns>
        [HttpGet("GetBadge{tenantId}/{id}")]
        public async Task<ActionResult<GetSpecificRecord<GetBadgeDto>>> GetBadge(int tenantId, int id)
        {
            var result = await _badgeService.GetBadgeByIdAsync(id, tenantId);
            return result.StatusCode == "200" ? Ok(result) : NotFound(result);
        }
    }
}