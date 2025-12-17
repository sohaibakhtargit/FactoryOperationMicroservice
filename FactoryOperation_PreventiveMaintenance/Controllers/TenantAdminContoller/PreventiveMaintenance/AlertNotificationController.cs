using FactoryOperation_PreventiveMaintenance.FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.PreventiveMaintenance;
using FactoryOpsApp.Application.DTOs;
using FactoryOpsApp.Domain.Entities.FactoryOpsTenants;
using FactoryOpsApp.Infrastructure.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FactoryOpsApp.API.Controllers.TenantAdminContoller.PreventiveMaintenance
{
    /// <summary>
    /// Alert Notification Management API
    /// Manages alert notifications and notification rules for tenants
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class AlertNotificationController : ControllerBase
    {
        private readonly IAlertNotificationService _alertNotificationService;
        public AlertNotificationController(IAlertNotificationService alertNotificationService)
        {
            _alertNotificationService = alertNotificationService;
        }

        /// <summary>
        /// Add alert notification
        /// Creates new alert notification rule for tenant monitoring
        /// </summary>
        /// <param name="dto">Alert notification data</param>
        /// <returns>Notification creation result</returns>
        /// <response code="200">Alert notification successfully created</response>
        /// <response code="400">Invalid input data provided</response>
        [HttpPost("Add-AlertNotification")]
        public async Task<IActionResult> AddAlertNotificationAsync([FromBody] AlertNotificationDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _alertNotificationService.AddAlertNotificationAsync(dto);
            return Ok(result);
        }

        /// <summary>
        /// Update alert notification
        /// Modifies existing alert notification rules and settings
        /// </summary>
        /// <param name="dto">Updated notification data</param>
        /// <returns>Update operation result</returns>
        /// <response code="200">Alert notification successfully updated</response>
        /// <response code="400">Invalid input data provided</response>
        [HttpPost("Update-AlertNotification")]
        public async Task<IActionResult> UpdateAlertRuleAsync([FromBody] AlertNotificationDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _alertNotificationService.UpdateAlertNotificationAsync(dto);
            return Ok(result);
        }

        /// <summary>
        /// Delete alert notification
        /// Removes alert notification rule from tenant configuration
        /// </summary>
        /// <param name="tenantId">Tenant identifier</param>
        /// <param name="alertId">Alert notification identifier</param>
        /// <returns>Deletion operation result</returns>
        /// <response code="200">Alert notification successfully deleted</response>
        [HttpPost("Delete-AlertNotification")]
        public async Task<IActionResult> DeleteAlertRuleAsync(int tenantId, int alertId)
        {
            var result = await _alertNotificationService.DeleteAlertNotificationAsync(alertId, tenantId);
            return Ok(result);
        }

        /// <summary>
        /// Get all alert notifications
        /// Retrieves all alert notification rules for a tenant
        /// </summary>
        /// <param name="tenantId">Tenant identifier</param>
        /// <returns>List of alert notifications</returns>
        /// <response code="200">Successfully retrieved alert notifications</response>
        [HttpGet("Get-AllAlertNotification")]
        public async Task<IActionResult> GetAllAlertRules(int tenantId)
        {
            var result = await _alertNotificationService.GetAllAlertNotificationsAsync(tenantId);
            return Ok(result);
        }

        /// <summary>
        /// Get alert notification by ID
        /// Retrieves specific alert notification details
        /// </summary>
        /// <param name="tenantId">Tenant identifier</param>
        /// <param name="alertId">Alert notification identifier</param>
        /// <returns>Alert notification details</returns>
        /// <response code="200">Successfully retrieved alert notification</response>
        [HttpGet("Get-AlertNotificationById")]
        public async Task<IActionResult> GetAlertRuleByIdAsync(int tenantId, int alertId)
        {
            var result = await _alertNotificationService.GetAlertNotificationByIdAsync(alertId, tenantId);
            return Ok(result);
        }
    }
}