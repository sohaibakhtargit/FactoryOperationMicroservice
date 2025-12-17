using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.TeamManagement;
using FactoryOps_AccessManagementService.FactoryOpsApp.Application.Common;
using FactoryOpsApp.Application.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FactoryOps_AccessManagementService.Controllers.TenantAdminController.TeamManagement
{
    /// <summary>
    /// Team Alert Notifications Management API
    /// Manages team notifications, alerts, and notification rules
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class TeamAlertNotificationsController : ControllerBase
    {
        private readonly ITeamAlertNotificationService _teamAlertNotificationService;

        public TeamAlertNotificationsController(ITeamAlertNotificationService teamAlertNotificationService)
        {
            _teamAlertNotificationService = teamAlertNotificationService;
        }

        /// <summary>
        /// Create team alert notification
        /// Creates new team notification or alert rule
        /// </summary>
        /// <param name="dto">Team alert notification data</param>
        /// <returns>Notification creation result</returns>
        /// <response code="200">Team alert notification successfully created</response>
        /// <response code="400">Invalid notification data provided</response>
        [HttpPost("CreateTeamAlertNotification")]
        public async Task<ActionResult<CommonResponseModel>> CreateTeamAlertNotification([FromBody] CreateTeamAlertNotificationDto dto)
        {
            var result = await _teamAlertNotificationService.AddTeamAlertNotificationAsync(dto);
            return result.StatusCode == "200" ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Update team alert notification
        /// Modifies existing team notification or alert rule
        /// </summary>
        /// <param name="dto">Updated team alert notification data</param>
        /// <returns>Update operation result</returns>
        /// <response code="200">Team alert notification successfully updated</response>
        /// <response code="400">Invalid notification data provided</response>
        [HttpPost("UpdateTeamAlertNotification")]
        public async Task<ActionResult<CommonResponseModel>> UpdateTeamAlertNotification([FromBody] UpdateTeamAlertNotificationDto dto)
        {
            var result = await _teamAlertNotificationService.UpdateTeamAlertNotificationAsync(dto);
            return result.StatusCode == "200" ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Delete team alert notification
        /// Removes team notification or alert rule
        /// </summary>
        /// <param name="id">Team alert notification identifier</param>
        /// <param name="tenantId">Tenant identifier</param>
        /// <returns>Deletion operation result</returns>
        /// <response code="200">Team alert notification successfully deleted</response>
        /// <response code="400">Invalid deletion request</response>
        [HttpPost("DeleteTeamAlertNotification")]
        public async Task<ActionResult<CommonResponseModel>> DeleteTeamAlertNotification(int id, int tenantId)
        {
            var result = await _teamAlertNotificationService.DeleteTeamAlertNotificationAsync(id, tenantId);
            return result.StatusCode == "200" ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Get team alert notifications
        /// Retrieves all team notifications and alert rules for tenant
        /// </summary>
        /// <param name="tenantId">Tenant identifier</param>
        /// <returns>List of team alert notifications</returns>
        /// <response code="200">Successfully retrieved team alert notifications</response>
        /// <response code="400">Invalid retrieval request</response>
        [HttpGet("GetTeamAlertNotifications")]
        public async Task<ActionResult<GetAllRecord<GetTeamAlertNotificationDto>>> GetTeamAlertNotifications(int tenantId)
        {
            var result = await _teamAlertNotificationService.GetAllTeamAlertNotificationsAsync(tenantId);
            return result.StatusCode == "200" ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Get team alert notification by ID
        /// Retrieves specific team notification details
        /// </summary>
        /// <param name="tenantId">Tenant identifier</param>
        /// <param name="id">Team alert notification identifier</param>
        /// <returns>Team alert notification details</returns>
        /// <response code="200">Successfully retrieved team alert notification</response>
        /// <response code="404">Team alert notification not found</response>
        [HttpGet("GetTeamAlertNotificationById")]
        public async Task<ActionResult<GetSpecificRecord<GetTeamAlertNotificationDto>>> GetTeamAlertNotification(int tenantId, int id)
        {
            var result = await _teamAlertNotificationService.GetTeamAlertNotificationByIdAsync(id, tenantId);
            return result.StatusCode == "200" ? Ok(result) : NotFound(result);
        }

        /// <summary>
        /// Mark notification as read
        /// Updates notification read status
        /// </summary>
        /// <param name="id">Team alert notification identifier</param>
        /// <param name="tenantId">Tenant identifier</param>
        /// <returns>Mark as read operation result</returns>
        /// <response code="200">Notification successfully marked as read</response>
        /// <response code="400">Invalid mark as read request</response>
        [HttpPost("MarkAsRead")]
        public async Task<ActionResult<CommonResponseModel>> MarkAsRead(int id, int tenantId)
        {
            var result = await _teamAlertNotificationService.MarkAsReadAsync(id, tenantId);
            return result.StatusCode == "200" ? Ok(result) : BadRequest(result);
        }
    }
}