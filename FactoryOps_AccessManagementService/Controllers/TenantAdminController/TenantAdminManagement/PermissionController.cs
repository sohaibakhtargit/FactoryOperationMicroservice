using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.TenantAdminManagement;
using FactoryOpsApp.Application.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FactoryOps_AccessManagementService.Controllers.TenantAdminController.TenantAdminManagement
{
    [AllowAnonymous]
    [Route("api/[controller]")]
    [ApiController]
    public class PermissionController : ControllerBase
    {
        private readonly IPermissionService _permissionService;

        public PermissionController(IPermissionService permissionService)
        {
            _permissionService = permissionService;
        }

        #region ------- MODULE ENDPOINTS --------

        [Authorize(Roles = "TenantAdmin")]
        [HttpPost("Add-Permission")]
        public async Task<IActionResult> AddPermission([FromBody] AddPermissionDto dto)
        {
            return Ok(await _permissionService.AddPermission(dto));
        }

        [Authorize(Roles = "TenantAdmin")]
        [HttpPost("Update-Permission")]
        public async Task<IActionResult> UpdatePermission([FromBody] AddPermissionDto dto)
        {
            return Ok(await _permissionService.UpdatePermission(dto));
        }

        [Authorize(Roles = "TenantAdmin")]
        [HttpPost("Delete-Permission")]
        public async Task<IActionResult> DeletePermission(int id, int TenantId)
        {
            return Ok(await _permissionService.DeletePermission(id, TenantId));
        }

        [HttpGet("Get-All-Permissions")]
        public IActionResult GetAllPermissions(int TenantId)
        {
            return Ok(_permissionService.GetAllPermissions(TenantId));
        }

        #endregion


        #region ------- SUB MODULE ENDPOINTS --------

        [Authorize(Roles = "TenantAdmin")]
        [HttpPost("Add-SubPermission")]
        public async Task<IActionResult> AddSubPermission([FromBody] AddSubPermissionDto dto)
        {
            return Ok(await _permissionService.AddSubPermission(dto));
        }

        [Authorize(Roles = "TenantAdmin")]
        [HttpPost("Update-SubPermission")]
        public async Task<IActionResult> UpdateSubPermission([FromBody] AddSubPermissionDto dto)
        {
            return Ok(await _permissionService.UpdateSubPermission(dto));
        }

        [Authorize(Roles = "TenantAdmin")]
        [HttpPost("Delete-SubPermission")]
        public async Task<IActionResult> DeleteSubPermission(int SubPermissionId, int TenantId)
        {
            return Ok(await _permissionService.DeleteSubPermission(SubPermissionId, TenantId));
        }

        [HttpGet("Get-All-Permission-With-Sub")]
        public async Task<IActionResult> GetPermissionWithSubList(int TenantId)
        {
            return Ok(await _permissionService.GetPermissionWithSubList(TenantId));
        }

        #endregion
    }
}
