using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.TenantAdminManagement;
using FactoryOpsApp.Application.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FactoryOps_AccessManagementService.Controllers.TenantAdminController.TenantAdminManagement
{
    /// <summary>
    /// Notification Rules Management API
    /// Manages notification rules, alert configurations, and notification settings
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationRulesController : ControllerBase
    {
        private readonly IFactoryNotificationRulesService _service;

        public NotificationRulesController(IFactoryNotificationRulesService service)
        {
            _service = service;
        }

        /// <summary>
        /// Add notification rules
        /// Creates new notification rule and alert configuration
        /// </summary>
        /// <param name="dto">Notification rule data</param>
        /// <returns>Rule creation result</returns>
        /// <response code="200">Notification rule successfully added</response>
        /// <response code="500">Internal server error during creation</response>
        [Authorize(Roles = "TenantAdmin")]
        [HttpPost]
        [Route("Add-Notification-Rules")]
        public async Task<IActionResult> Create([FromBody] NotificationRuleDto dto)
        {
            var result = await _service.CreateAsync(dto);
            if (result.StatusCode == "200")
                return Ok(result);
            return StatusCode(500, result);
        }

        /// <summary>
        /// Get all notifications
        /// Retrieves complete list of all notification rules for tenant
        /// </summary>
        /// <param name="tenantId">Tenant identifier</param>
        /// <returns>List of notification rules</returns>
        /// <response code="200">Successfully retrieved all notifications</response>
        /// <response code="500">Internal server error during retrieval</response>
        [HttpGet("getAll-notification")]
        public async Task<IActionResult> GetAll([FromQuery] int tenantId)
        {
            var result = await _service.GetAllAsync(tenantId);
            if (result.StatusCode == "200")
                return Ok(result);
            return StatusCode(500, result);
        }

        /// <summary>
        /// Get notification by ID
        /// Retrieves specific notification rule details
        /// </summary>
        /// <param name="id">Notification rule identifier</param>
        /// <param name="tenantId">Tenant identifier</param>
        /// <returns>Notification rule details</returns>
        /// <response code="200">Successfully retrieved notification</response>
        /// <response code="404">Notification rule not found</response>
        /// <response code="500">Internal server error during retrieval</response>
        [HttpGet("getNotificationBy_Id")]
        public async Task<IActionResult> GetById(int id, [FromQuery] int tenantId)
        {
            var result = await _service.GetByIdAsync(id, tenantId);
            if (result.StatusCode == "200")
                return Ok(result);
            if (result.StatusCode == "404")
                return NotFound(result);
            return StatusCode(500, result);
        }

        /// <summary>
        /// Update notification rules
        /// Modifies existing notification rule configuration
        /// </summary>
        /// <param name="dto">Updated notification rule data</param>
        /// <returns>Update operation result</returns>
        /// <response code="200">Notification rule successfully updated</response>
        /// <response code="500">Internal server error during update</response>
        [Authorize(Roles = "TenantAdmin")]
        [HttpPost]
        [Route("Update-Notification-Rules")]
        public async Task<IActionResult> Update([FromBody] NotificationRuleDto dto)
        {
            var result = await _service.UpdateAsync(dto);
            if (result.StatusCode == "200")
                return Ok(result);
            return StatusCode(500, result);
        }

        /// <summary>
        /// Delete notification
        /// Removes notification rule from system
        /// </summary>
        /// <param name="id">Notification rule identifier</param>
        /// <param name="tenantId">Tenant identifier</param>
        /// <returns>Deletion operation result</returns>
        /// <response code="200">Notification successfully deleted</response>
        /// <response code="500">Internal server error during deletion</response>
        [Authorize(Roles = "TenantAdmin")]
        [HttpPost("Delete-Notification")]
        public async Task<IActionResult> Delete(int id, int tenantId)
        {
            var result = await _service.DeleteAsync(id, tenantId);
            if (result.StatusCode == "200")
                return Ok(result);
            return StatusCode(500, result);
        }
    }
}