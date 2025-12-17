using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.TenantAdminManagement;
using Microsoft.AspNetCore.Mvc;

namespace FactoryOps_AccessManagementService.Controllers.TenantAdminController.TenantAdminManagement
{
    /// <summary>
    /// Role Permission Mapping API
    /// Manages role-permission mappings and access control configurations
    /// </summary>
    //[Authorize(Roles = "SuperAdmin")]
    [Route("api/[controller]")]
    [ApiController]
    public class RolePermissionMappingController : ControllerBase
    {
        private readonly IRolePermissionMappingService _service;

        public RolePermissionMappingController(IRolePermissionMappingService service)
        {
            _service = service;
        }

        /// <summary>
        /// Get permissions by role ID
        /// Retrieves all permissions mapped to specific role
        /// </summary>
        /// <param name="roleId">Role identifier</param>
        /// <param name="TenantId">Tenant identifier</param>
        /// <returns>List of role permissions</returns>
        /// <response code="200">Successfully retrieved role permissions</response>
        [HttpGet("GetByRole/{roleId}")]
        public IActionResult GetPermissionsByRoleId(int roleId, int TenantId)
        {
            var result = _service.GetPermissionsByRoleId(roleId, TenantId);
            return Ok(result);
        }
    }
}