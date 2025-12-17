using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Services.SuperAdmin.Configuration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FactoryOps_AccessManagementService.Controllers.MigrationController
{

    /// <summary>
    /// Database Migration API
    /// Manages database schema migrations and tenant-specific schema updates
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class MigrationController : ControllerBase
    {
        private readonly ITenantMigrationService _migrationService;

        public MigrationController(ITenantMigrationService migrationService)
        {
            _migrationService = migrationService;
        }

        /// <summary>
        /// Apply migration system-wide
        /// Applies database schema migrations across all tenants
        /// </summary>
        /// <returns>Migration operation result</returns>
        /// <response code="200">Migration successfully applied</response>
        [Authorize(Roles = "SuperAdmin")]
        [HttpPost("apply-migration")]
        public async Task<IActionResult> ApplyMigrationAsync()
        {
            var data = await _migrationService.ApplySchemaMigrationAsync();
            return Ok(data);
        }

        /// <summary>
        /// Apply migration tenant-wise
        /// Applies database schema migrations for specific tenant
        /// </summary>
        /// <param name="TenantId">Tenant identifier</param>
        /// <returns>Migration operation result</returns>
        /// <response code="200">Tenant migration successfully applied</response>
        [Authorize(Roles = "SuperAdmin")]
        [HttpPost("apply-migration-TenantWise")]
        public async Task<IActionResult> ApplyMigrationByTenantAsync(int TenantId)
        {
            var data = await _migrationService.ApplySchemaMigrationByTenantAsync(TenantId);
            return Ok(data);
        }
    }
}