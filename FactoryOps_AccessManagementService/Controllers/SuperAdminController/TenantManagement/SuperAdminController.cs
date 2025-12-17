using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Services.SuperAdmin.TenantManagement;
using FactoryOps_AccessManagementService.FactoryOpsApp.Application.Common;
using FactoryOpsApp.Application.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FactoryOps_AccessManagementService.Controllers.SuperAdminController.TenantManagement
{
    /// <summary>
    /// Super Admin Tenant Management API
    /// Provides complete tenant lifecycle management and administrative controls
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class SuperAdminController : ControllerBase
    {
        private readonly ITenantService _iTenantService;
        public SuperAdminController(ITenantService iTenantService)
        {
            _iTenantService = iTenantService;
        }

        /// <summary>
        /// Add new tenant to the system
        /// Creates a new tenant with configured settings and permissions
        /// </summary>
        /// <param name="dto">Tenant configuration data</param>
        /// <returns>Tenant creation result</returns>
        /// <response code="200">Tenant successfully added</response>
        //[Authorize(Roles = "SuperAdmin")]
        [HttpPost]
        [Route("Add-Tenant")]
        public async Task<IActionResult> AddTenant([FromForm] AddTenantDto dto)
        {
            var result = await _iTenantService.AddTenant(dto);
            return Ok(result);
        }

        /// <summary>
        /// Get all tenants
        /// Retrieves complete list of all tenants in the system
        /// </summary>
        /// <returns>List of all tenants</returns>
        /// <response code="200">Successfully retrieved all tenants</response>
        [HttpGet]
        [Route("get-Tenants")]
        public async Task<IActionResult> GetAllTenants()
        {
            var result = await _iTenantService.GetAllTenants();
            return Ok(result);
        }

        /// <summary>
        /// Update tenant information
        /// Modifies tenant configuration, settings, and permissions
        /// </summary>
        /// <param name="dto">Updated tenant data</param>
        /// <returns>Update operation result</returns>
        /// <response code="200">Tenant successfully updated</response>
        [Authorize(Roles = "SuperAdmin")]
        [HttpPost]
        [Route("update-Tenants")]
        public async Task<IActionResult> UpdateTenants([FromForm] AddTenantDto dto)
        {
            var result = await _iTenantService.UpdateTenants(dto);
            return Ok(result);
        }

        /// <summary>
        /// Delete tenant from system
        /// Permanently removes a tenant and associated data
        /// </summary>
        /// <param name="Id">Tenant identifier</param>
        /// <returns>Deletion operation result</returns>
        /// <response code="200">Tenant successfully deleted</response>
        /// <response code="500">Internal server error during deletion</response>
        /// 
        //[Authorize(Roles = "SuperAdmin")]
        [HttpPost]
        [Route("update-modules")]
        public async Task<IActionResult> UpdateTenantModules([FromBody] UpdateTenantModulesDto dto)
        {
            var result = await _iTenantService.UpdateTenantModulesAsync(dto);
            return Ok(result);
        }

        [Authorize(Roles = "SuperAdmin")]
        [HttpPost]
        [Route("Delete-Tenants")]
        public async Task<IActionResult> DeleteTenants(int Id)
        {
            try
            {
                var result = await _iTenantService.DeleteTenants(Id);
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
        /// Change tenant status
        /// Toggles tenant active/inactive status
        /// </summary>
        /// <param name="Id">Tenant identifier</param>
        /// <returns>Status change result</returns>
        /// <response code="200">Tenant status successfully changed</response>
        [Authorize(Roles = "SuperAdmin")]
        [HttpPost]
        [Route("Change-Status")]
        public async Task<IActionResult> UpdateTenants(int Id)
        {
            var result = await _iTenantService.ChangeTenants(Id);
            return Ok(result);
        }

        /// <summary>
        /// Force logout all tenant users
        /// Logs out all active users from the specified tenant
        /// </summary>
        /// <param name="Id">Tenant identifier</param>
        /// <returns>Force logout operation result</returns>
        /// <response code="200">All users successfully logged out</response>
        [Authorize(Roles = "SuperAdmin")]
        [HttpPost]
        [Route("force-logout-tenant")]
        public async Task<IActionResult> ForceLogout(int Id)
        {
            var result = await _iTenantService.ForceLogout(Id);
            return Ok(result);
        }

        /// <summary>
        /// Suspend tenant operations
        /// Temporarily suspends all tenant activities and access
        /// </summary>
        /// <param name="Id">Tenant identifier</param>
        /// <returns>Suspension operation result</returns>
        /// <response code="200">Tenant successfully suspended</response>
        [Authorize(Roles = "SuperAdmin")]
        [HttpPost]
        [Route("suspend-tenant")]
        public async Task<IActionResult> Suspend(int Id)
        {
            var result = await _iTenantService.Suspend(Id);
            return Ok(result);
        }

        [HttpGet]
        [Route("get-moduleList")]
        public async Task<IActionResult> GetAllModuleList()
        {
            var result = await _iTenantService.GetAllModuleList();
            return Ok(result);
        }
        [HttpGet]
        [Route("Get-AllModuleList")]
        public async Task<IActionResult> GetAllModuleAsync(int TenantId)
        {
            var result = await _iTenantService.GetAllModuleAsync(TenantId);
            return Ok(result);
        }
    }
}