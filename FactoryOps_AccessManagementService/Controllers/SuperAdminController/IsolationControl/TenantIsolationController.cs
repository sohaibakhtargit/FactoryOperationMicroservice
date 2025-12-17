using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Services.SuperAdmin.IsolationControl;
using FactoryOpsApp.Application.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace FactoryOps_AccessManagementService.Controllers.SuperAdminController.IsolationControl
{
    /// <summary>
    /// Tenant Isolation Management API for Super Admin
    /// Manages tenant data segregation, security policies, and compliance metrics
    /// </summary>
    [Route("api/superadmin/[controller]")]
    [ApiController]
    //  [Authorize(Roles = "SuperAdmin")] 
    public class TenantIsolationController : ControllerBase
    {
        private readonly ITenantIsolationService _tenantIsolationService;

        public TenantIsolationController(ITenantIsolationService tenantIsolationService)
        {
            _tenantIsolationService = tenantIsolationService;
        }

        /// <summary>
        /// Configure or update tenant isolation settings
        /// Establishes data isolation boundaries and security policies for tenant data segregation
        /// </summary>
        /// <param name="dto">Tenant isolation configuration data</param>
        /// <returns>Operation status</returns>
        /// <response code="200">Isolation settings successfully configured or updated</response>
        /// <response code="400">Invalid input data or conflicting isolation rules</response>
        [HttpPost("add-or-update")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> AddOrUpdateIsolation([FromForm] AddTenantIsolationDto dto)
        {
            var result = await _tenantIsolationService.AddOrUpdateIsolationAsync(dto);
            return Ok(result);
        }

        /// <summary>
        /// Get isolation settings for a specific tenant
        /// Retrieves all isolation configurations and security policies for a tenant
        /// </summary>
        /// <param name="tenantId">The unique identifier of the tenant</param>
        /// <returns>Tenant isolation settings</returns>
        /// <response code="200">Successfully retrieved isolation configuration</response>
        /// <response code="404">No isolation settings found for this tenant</response>
        [HttpGet("get/{tenantId}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetIsolation(int tenantId)
        {
            var result = await _tenantIsolationService.GetIsolationByTenantIdAsync(tenantId);
            if (result == null)
            {
                return Ok(new { StatusCode = "200", StatusMessage = "Please Add Branding Isolation", Data = result });
            }
            return Ok(new { StatusCode = "200", StatusMessage = "Success", Data = result });
        }

        /// <summary>
        /// Update audit compliance metrics for a tenant
        /// Modifies compliance metrics to track regulatory requirements and security standards
        /// </summary>
        /// <param name="dto">Audit compliance metrics data</param>
        /// <returns>Update operation result</returns>
        /// <response code="200">Audit compliance metrics successfully updated</response>
        /// <response code="400">Update failed due to invalid data or validation errors</response>
        [HttpPost("FactoryAuditComplianceMetrics")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> UpdateComplianceAudit([FromBody] UpdateAuditComplianceMetricsDto dto)
        {
            var result = await _tenantIsolationService.UpsertAuditComplianceMetricAsync(dto);

            if (result == null || result.StatusCode != "200")
            {
                return BadRequest(new { StatusCode = "400", StatusMessage = "Update failed." });
            }

            return Ok(result);
        }

        /// <summary>
        /// Get all audit compliance metrics
        /// Retrieves comprehensive compliance status and metrics across all tenants
        /// </summary>
        /// <returns>List of all audit compliance metrics</returns>
        /// <response code="200">Successfully retrieved all compliance metrics</response>
        /// <response code="404">No audit compliance metrics found in system</response>
        [HttpGet]
        [Route("GetAllAudiComplianceMetrics")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public IActionResult GetAuditComplianceMetrics()
        {
            var result = _tenantIsolationService.GetAuditComplianceMetricsAsync();
            if (result == null)
                return NotFound(new { StatusCode = "404", StatusMessage = "No audit compliance metrics found." });

            return Ok(new { StatusCode = "200", StatusMessage = "Success", Data = result });
        }
    }
}