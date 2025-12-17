using Microsoft.AspNetCore.Mvc;
using FactoryOpsApp.Application.DTOs;
using FactoryOpsApp.Domain.Entities.FactoryOpsTenants;
using FactoryOpsApp_IOTDevices.FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.IOTDevices;

namespace FactoryOpsApp_IOTDevices.Controllers.IOTDevices
{
    /// <summary>
    /// Alert Rule Management API
    /// Manages alert rules and monitoring configurations for tenants
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class AlertRuleController : ControllerBase
    {
        private readonly IAlertRuleService _alertRuleService;

        public AlertRuleController(IAlertRuleService alertRuleService)
        {
            _alertRuleService = alertRuleService;
        }

        /// <summary>
        /// Add alert rule
        /// Creates new alert rule for tenant monitoring and notifications
        /// </summary>
        /// <param name="dto">Alert rule configuration data</param>
        /// <returns>Alert rule creation result</returns>
        /// <response code="200">Alert rule successfully created</response>
        /// <response code="400">Invalid input data provided</response>
        [HttpPost("Add-AlertRule")]
        public async Task<IActionResult> AddAlertRuleAsync([FromBody] AlertRuleDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _alertRuleService.AddAlertRuleAsync(dto);
            return Ok(result);
        }

        /// <summary>
        /// Update alert rule
        /// Modifies existing alert rule configuration and settings
        /// </summary>
        /// <param name="dto">Updated alert rule data</param>
        /// <returns>Update operation result</returns>
        /// <response code="200">Alert rule successfully updated</response>
        /// <response code="400">Invalid input data provided</response>
        [HttpPost("Update-AlertRule")]
        public async Task<IActionResult> UpdateAlertRuleAsync([FromBody] AlertRuleDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _alertRuleService.UpdateAlertRuleAsync(dto);
            return Ok(result);
        }

        /// <summary>
        /// Delete alert rule
        /// Removes alert rule from tenant configuration
        /// </summary>
        /// <param name="tenantId">Tenant identifier</param>
        /// <param name="alertRuleId">Alert rule identifier</param>
        /// <returns>Deletion operation result</returns>
        /// <response code="200">Alert rule successfully deleted</response>
        [HttpPost("Delete-AlertRule")]
        public async Task<IActionResult> DeleteAlertRuleAsync(int tenantId, int alertRuleId)
        {
            var result = await _alertRuleService.DeleteAlertRuleAsync(alertRuleId, tenantId);
            return Ok(result);
        }

        /// <summary>
        /// Get all alert rules
        /// Retrieves all alert rules for a tenant with optional status filtering
        /// </summary>
        /// <param name="tenantId">Tenant identifier</param>
        /// <param name="statusFilter">Optional alert status filter</param>
        /// <returns>List of alert rules</returns>
        /// <response code="200">Successfully retrieved alert rules</response>
        [HttpGet("Get-AllAlertRules")]
        public async Task<IActionResult> GetAllAlertRules(int tenantId, AlertStatusEnum? statusFilter = null)
        {
            var result = await _alertRuleService.GetAllAlertRulesAsync(tenantId, statusFilter);
            return Ok(result);
        }

        /// <summary>
        /// Get alert rule by ID
        /// Retrieves specific alert rule details and configuration
        /// </summary>
        /// <param name="tenantId">Tenant identifier</param>
        /// <param name="alertRuleId">Alert rule identifier</param>
        /// <returns>Alert rule details</returns>
        /// <response code="200">Successfully retrieved alert rule</response>
        [HttpGet("Get-AlertRuleById")]
        public async Task<IActionResult> GetAlertRuleByIdAsync(int tenantId, int alertRuleId)
        {
            var result = await _alertRuleService.GetAlertRuleByIdAsync(alertRuleId, tenantId);
            return Ok(result);
        }
    }
}