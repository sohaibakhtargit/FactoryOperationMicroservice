using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.TenantAdminManagement;
using FactoryOpsApp.Application.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FactoryOps_AccessManagementService.Controllers.TenantAdminController.TenantAdminManagement
{
    /// <summary>
    /// Roles Management API
    /// Manages user roles, role configurations, and role-based access controls
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class RolesController : ControllerBase
    {
        private readonly IRoleService _roleService;

        public RolesController(IRoleService roleService)
        {
            _roleService = roleService;
        }

        /// <summary>
        /// Add role
        /// Creates new user role with specified permissions
        /// </summary>
        /// <param name="dto">Role data</param>
        /// <returns>Role creation result</returns>
        /// <response code="200">Role successfully added</response>
        [Authorize]
        [HttpPost("Add")]
        public async Task<IActionResult> AddRole([FromBody] AddRoleDto dto)
        {
            var result = await _roleService.AddRole(dto);
            return Ok(result);
        }

        /// <summary>
        /// Update role
        /// Modifies existing role configuration and settings
        /// </summary>
        /// <param name="dto">Updated role data</param>
        /// <returns>Update operation result</returns>
        /// <response code="200">Role successfully updated</response>
        [Authorize]
        [HttpPost("Update")]
        public async Task<IActionResult> UpdateRole([FromBody] AddRoleDto dto)
        {
            var result = await _roleService.UpdateRole(dto);
            return Ok(result);
        }

        /// <summary>
        /// Delete role
        /// Removes role from system access controls
        /// </summary>
        /// <param name="id">Role identifier</param>
        /// <param name="TenantId">Tenant identifier</param>
        /// <returns>Deletion operation result</returns>
        /// <response code="200">Role successfully deleted</response>
        [Authorize]
        [HttpPost("Delete")]
        public async Task<IActionResult> DeleteRole(int id, int TenantId)
        {
            var result = await _roleService.DeleteRole(id, TenantId);
            return Ok(result);
        }

        /// <summary>
        /// Get all roles and permissions
        /// Retrieves complete list of all roles with their permissions
        /// </summary>
        /// <param name="TenantId">Tenant identifier</param>
        /// <returns>List of roles with permissions</returns>
        /// <response code="200">Successfully retrieved all roles and permissions</response>
        [HttpGet("GetAll-Role-Permissions")]
        public IActionResult GetAllRoles(int TenantId)
        {
            var result = _roleService.GetAllRoles(TenantId);
            return Ok(result);
        }
    }
}