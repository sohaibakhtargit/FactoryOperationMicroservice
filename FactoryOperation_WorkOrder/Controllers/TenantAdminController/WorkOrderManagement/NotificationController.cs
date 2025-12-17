using FactoryOperation_WorkOrder.FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.WorkOrderServices;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FactoryOperation_WorkOrder.Controllers.TenantAdminController.WorkOrderManagement
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationQueryService _notificationService;

        public NotificationController(INotificationQueryService notificationService)
        {
            _notificationService = notificationService;
        }

        [HttpGet("getAll")]
        public async Task<IActionResult> GetAll(int tenantId)
        {
            var data = await _notificationService.GetAllNotificationsAsync(tenantId);
            return Ok(data);
        }

        [HttpGet("getAllWorkOrder-Notification")]
        public async Task<IActionResult> GetAllWONotification(int tenantId)
        {
            var data = await _notificationService.GetAllWONotification(tenantId);
            return Ok(data);
        }

        [HttpGet("getUnread-Notification")]
        public async Task<IActionResult> GetUnread(int tenantId)
        {
            var data = await _notificationService.GetUnreadNotificationsAsync(tenantId);
            return Ok(data);
        }

        [HttpGet("get-userNotification")]
        public async Task<IActionResult> GetUserNotifications(int tenantId, int userId)
        {
            var data = await _notificationService.GetUserNotificationsAsync(tenantId, userId);
            return Ok(data);
        }

        [HttpPost("mark-read")]
        public async Task<IActionResult> MarkAsRead(int tenantId, int notificationId)
        {
            var success = await _notificationService.MarkAsReadAsync(notificationId, tenantId);
            return success ? Ok("Marked as read") : NotFound("Notification not found");
        }
    }
}
