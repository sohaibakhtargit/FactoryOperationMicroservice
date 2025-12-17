using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.TenantAdminManagement;
using FactoryOps_AccessManagementService.FactoryOpsApp.Application.Common;
using FactoryOpsApp.Application.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FactoryOps_AccessManagementService.Controllers.TenantAdminController.TenantAdminManagement
{
    /// <summary>
    /// Tenant Admin Management API
    /// Manages tenant users, user operations, and tenant-level user administration
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class TenantAdminController : ControllerBase
    {
        private readonly IFactoryUserService _iuserService;
        public TenantAdminController(IFactoryUserService iuserService)
        {
            _iuserService = iuserService;
        }

        /// <summary>
        /// Add user
        /// Creates new user account within tenant
        /// </summary>
        /// <param name="addUser">User data</param>
        /// <returns>User creation result</returns>
        /// <response code="200">User successfully added</response>
        /// <response code="500">Internal server error during creation</response>
        [Authorize(Roles = "TenantAdmin")]
        [HttpPost("add-user")]
        public async Task<IActionResult> AddNewUser([FromBody] UserResponseDto addUser)
        {
            try
            {
                var result = await _iuserService.AddNewUser(addUser);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new CommonResponseModel
                {
                    StatusCode = "500",
                    StatusMessage = $"Internal Server Error: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Edit user
        /// Modifies existing user account information
        /// </summary>
        /// <param name="addUser">Updated user data</param>
        /// <returns>Update operation result</returns>
        /// <response code="200">User successfully updated</response>
        /// <response code="500">Internal server error during update</response>
        [Authorize(Roles = "TenantAdmin")]
        [HttpPost("edit-user")]
        public async Task<IActionResult> EditExistingUser([FromBody] UserResponseDto addUser)
        {
            try
            {
                var result = await _iuserService.EditExistingUser(addUser);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new CommonResponseModel
                {
                    StatusCode = "500",
                    StatusMessage = $"Internal Server Error: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Get users
        /// Retrieves all users for tenant
        /// </summary>
        /// <param name="TenantId">Tenant identifier</param>
        /// <returns>List of tenant users</returns>
        /// <response code="200">Successfully retrieved users</response>
        /// <response code="500">Internal server error during retrieval</response>
        [HttpGet("get-users")]
        public async Task<IActionResult> GetAllUsers(int TenantId)
        {
            try
            {
                var result = _iuserService.GetAllUsers(TenantId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new CommonResponseModel
                {
                    StatusCode = "500",
                    StatusMessage = $"Internal Server Error: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Delete user
        /// Removes user account from tenant
        /// </summary>
        /// <param name="Id">User identifier</param>
        /// <param name="TenantId">Tenant identifier</param>
        /// <returns>Deletion operation result</returns>
        /// <response code="200">User successfully deleted</response>
        /// <response code="500">Internal server error during deletion</response>
        [Authorize(Roles = "TenantAdmin")]
        [HttpPost]
        [Route("Delete-Users")]
        public async Task<IActionResult> DeleteUser(int Id, int TenantId)
        {
            try
            {
                var result = await _iuserService.DeleteUser(Id, TenantId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new CommonResponseModel
                {
                    StatusCode = "500",
                    StatusMessage = $"Internal Server Error: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Force logout user
        /// Forces user logout from all active sessions
        /// </summary>
        /// <param name="Id">User identifier</param>
        /// <param name="TenantId">Tenant identifier</param>
        /// <returns>Force logout operation result</returns>
        /// <response code="200">User successfully logged out</response>
        [Authorize(Roles = "TenantAdmin")]
        [HttpPost]
        [Route("force-logout-user")]
        public async Task<IActionResult> ForceLogout(int Id, int TenantId)
        {
            var result = await _iuserService.ForceLogout(Id, TenantId);
            return Ok(result);
        }

        /// <summary>
        /// Suspend user
        /// Suspends user account access temporarily
        /// </summary>
        /// <param name="dto">User suspension data</param>
        /// <returns>Suspension operation result</returns>
        /// <response code="200">User successfully suspended</response>
        [Authorize(Roles = "TenantAdmin")]
        [HttpPost]
        [Route("suspend-user")]
        public async Task<IActionResult> Suspend([FromBody] SuspendUserDto dto)
        {
            var result = await _iuserService.Suspend(dto);
            return Ok(result);
        }

        /// <summary>
        /// Get managers
        /// Retrieves all manager users for tenant
        /// </summary>
        /// <param name="tenantId">Tenant identifier</param>
        /// <returns>List of managers</returns>
        /// <response code="200">Successfully retrieved managers</response>
        /// <response code="500">Internal server error during retrieval</response>
        [HttpGet("get-managers")]
        public IActionResult GetAllManagers(int tenantId)
        {
            try
            {
                var result = _iuserService.GetAllManagersByTenantAsync(tenantId);
                return Ok(new
                {
                    StatusCode = 200,
                    StatusMessage = "Managers fetched successfully",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    StatusCode = 500,
                    StatusMessage = "An error occurred",
                    Error = ex.Message
                });
            }
        }

        /// <summary>
        /// Get all members
        /// Retrieves all non-manager users for tenant
        /// </summary>
        /// <param name="tenantId">Tenant identifier</param>
        /// <returns>List of non-manager users</returns>
        /// <response code="200">Successfully retrieved all members</response>
        [HttpGet]
        [Route("get-AllMembers")]
        public IActionResult GetAllMembers(int tenantId)
        {
            var data = _iuserService.GetAllUsersExceptManager(tenantId);
            return Ok(data);
        }
    }
}