using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.TeamManagement;
using FactoryOps_AccessManagementService.FactoryOpsApp.Application.Common;
using FactoryOpsApp.Application.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FactoryOps_AccessManagementService.Controllers.TenantAdminController.TeamManagement
{
    /// <summary>
    /// User Badge Management API
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class UserBadgesController : ControllerBase
    {
        private readonly IUserBadgeService _userBadgeService;

        public UserBadgesController(IUserBadgeService userBadgeService)
        {
            _userBadgeService = userBadgeService;
        }

        /// <summary>
        /// Add a badge to a user
        /// </summary>
        /// <param name="dto">User badge data</param>
        /// <returns>Operation status</returns>
        [HttpPost("CreateUserBadge")]
        public async Task<ActionResult<CommonResponseModel>> CreateUserBadge([FromBody] UserBadgeDto dto)
        {
            var result = await _userBadgeService.AddUserBadgeAsync(dto);
            return result.StatusCode == "200" ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Award a badge to a user
        /// </summary>
        /// <param name="dto">Award badge data</param>
        /// <returns>Operation status</returns>
        [HttpPost("award")]
        public async Task<ActionResult<CommonResponseModel>> AwardUserBadge([FromBody] AwardUserBadgeDto dto)
        {
            var result = await _userBadgeService.AwardUserBadgeAsync(dto);
            return result.StatusCode == "200" ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Update user badge progress
        /// </summary>
        /// <param name="dto">Progress update data</param>
        /// <returns>Operation status</returns>
        [HttpPost("progress")]
        public async Task<ActionResult<CommonResponseModel>> UpdateUserBadgeProgress([FromBody] UpdateUserBadgeProgressDto dto)
        {
            var result = await _userBadgeService.UpdateUserBadgeProgressAsync(dto);
            return result.StatusCode == "200" ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Delete a user badge
        /// </summary>
        /// <param name="id">User badge ID</param>
        /// <param name="tenantId">Tenant ID</param>
        /// <returns>Deletion status</returns>
        [HttpPost("DeleteUserBadge")]
        public async Task<ActionResult<CommonResponseModel>> DeleteUserBadge(int id, int tenantId)
        {
            var result = await _userBadgeService.DeleteUserBadgeAsync(id, tenantId);
            return result.StatusCode == "200" ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Get all user badges for a tenant
        /// </summary>
        /// <param name="tenantId">Tenant ID</param>
        /// <returns>List of user badges</returns>
        [HttpGet("GetUserBadges{tenantId}")]
        public async Task<ActionResult<GetAllRecord<GetUserBadgeDto>>> GetUserBadges(int tenantId)
        {
            var result = await _userBadgeService.GetAllUserBadgesAsync(tenantId);
            return result.StatusCode == "200" ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Get a specific user badge by ID
        /// </summary>
        /// <param name="tenantId">Tenant ID</param>
        /// <param name="id">User badge ID</param>
        /// <returns>User badge details</returns>
        [HttpGet("{tenantId}/{id}")]
        public async Task<ActionResult<GetSpecificRecord<GetUserBadgeDto>>> GetUserBadge(int tenantId, int id)
        {
            var result = await _userBadgeService.GetUserBadgeByIdAsync(id, tenantId);
            return result.StatusCode == "200" ? Ok(result) : NotFound(result);
        }

        /// <summary>
        /// Get all badges for a specific user
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="tenantId">Tenant ID</param>
        /// <returns>List of user badges</returns>
        [HttpGet("user/{userId}/{tenantId}")]
        public async Task<ActionResult<GetAllRecord<GetUserBadgeDto>>> GetUserBadgesByUser(int userId, int tenantId)
        {
            var result = await _userBadgeService.GetUserBadgesByUserAsync(userId, tenantId);
            return result.StatusCode == "200" ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Get all users who have a specific badge
        /// </summary>
        /// <param name="badgeId">Badge ID</param>
        /// <param name="tenantId">Tenant ID</param>
        /// <returns>List of user badges</returns>
        [HttpGet("badge/{badgeId}/{tenantId}")]
        public async Task<ActionResult<GetAllRecord<GetUserBadgeDto>>> GetUserBadgesByBadge(int badgeId, int tenantId)
        {
            var result = await _userBadgeService.GetUserBadgesByBadgeAsync(badgeId, tenantId);
            return result.StatusCode == "200" ? Ok(result) : BadRequest(result);
        }
    }
}