using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Services.SuperAdmin.UserManagement;
using FactoryOpsApp.Application.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FactoryOps_AccessManagementService.Controllers.SuperAdminController.UserManagement
{
    /// <summary>
    /// Global Users Management API
    /// Provides comprehensive user management across all tenants and systems
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class GlobalUsersController : ControllerBase
    {
        private readonly IGlobalUserService _iglobaluserService;
        public GlobalUsersController(IGlobalUserService iglobaluserService)
        {
            _iglobaluserService = iglobaluserService;
        }

        /// <summary>
        /// Get all global users
        /// Retrieves complete list of all users across all tenants
        /// </summary>
        /// <returns>List of all global users</returns>
        /// <response code="200">Successfully retrieved all global users</response>
        [HttpGet("getAll-globalUsers")]
        public IActionResult getAllGlobalUsers()
        {
            var result = _iglobaluserService.GetAllGlobalUsers();
            return Ok(result);
        }

        /// <summary>
        /// Get all suspended users
        /// Retrieves list of users with suspended access across the system
        /// </summary>
        /// <returns>List of suspended users</returns>
        /// <response code="200">Successfully retrieved suspended users</response>
        [HttpGet("getAll-SuspendUsers")]
        public IActionResult getAllSuspendUsers()
        {
            var result = _iglobaluserService.GetAllSuspendUsers();
            return Ok(result);
        }

        /// <summary>
        /// Suspend or unsuspend global user
        /// Toggles user suspension status to restrict or restore system access
        /// </summary>
        /// <param name="dto">User suspension data</param>
        /// <returns>Suspension operation result</returns>
        /// <response code="200">User suspension status successfully updated</response>
        [Authorize(Roles = "SuperAdmin")]
        [HttpPost("Suspend-globalUsers")]
        public async Task<IActionResult> ToggleSuspend(SuspendGlobalUserDto dto)
        {
            var result = await _iglobaluserService.ToggleSuspend(dto);
            return Ok(result);
        }

        /// <summary>
        /// Force logout user from all sessions
        /// Immediately terminates all active user sessions across devices
        /// </summary>
        /// <param name="Id">User identifier</param>
        /// <returns>Force logout operation result</returns>
        /// <response code="200">User successfully logged out from all sessions</response>
        [Authorize(Roles = "SuperAdmin")]
        [HttpPost]
        [Route("force-logout-user")]
        public async Task<IActionResult> ForceLogout(int Id)
        {
            var result = await _iglobaluserService.ForceLogout(Id);
            return Ok(result);
        }

        /// <summary>
        /// Get detailed user information
        /// Retrieves comprehensive user details including tenant association and profile data
        /// </summary>
        /// <param name="tenantId">Tenant identifier</param>
        /// <param name="Email">User email address</param>
        /// <returns>Detailed user information</returns>
        /// <response code="200">Successfully retrieved user information</response>
        [HttpGet]
        [Route("Get-UsersInfo")]
        public IActionResult GetInfoOfUsers(int tenantId, string Email)
        {
            var result = _iglobaluserService.GetInfoOfUsers(tenantId, Email);
            return Ok(result);
        }
        [HttpPost]
        [Route("Update-UserProfile")]
        public async Task<IActionResult> UpdateUserProfile([FromForm] UpdateUserProfileDto dto)
        {
            var result = await _iglobaluserService.UpdateUserProfile(dto);
            return Ok(result);
        }
        [HttpGet]
        [Route("Get-SuperAdminInfo")]
        public IActionResult GetSuperAdminInfo(int userId)
        {
            var result = _iglobaluserService.GetSuperAdminInfo(userId);
            return Ok(result);
        }
        [HttpPost("Update-SuperAdminProfile")]
        public async Task<IActionResult> UpdateSuperAdminProfileInfo([FromForm] UpdateSuperAdminProfileDto dto)
        {
            var result = await _iglobaluserService.UpdateSuperAdminProfile(dto);
            return StatusCode(int.Parse(result.StatusCode ?? "500"), result);
        }
        [HttpGet]
        [Route("Get-TenantInfo")]
        public async Task<IActionResult> GetTenantInfo(int tenantId)
        {
            var result = _iglobaluserService.GetTenantInfo(tenantId);
            return Ok(result);
        }
        [HttpPost("Update-TenantProfile")]
        public async Task<IActionResult> UpdateTenantProfile([FromForm] UpdateTenantProfileDto dto)
        {
            var result = await _iglobaluserService.UpdateTenantProfile(dto);
            return StatusCode(int.Parse(result.StatusCode ?? "500"), result);
        }
    }
}