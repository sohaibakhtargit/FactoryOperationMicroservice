using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.Notification;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FactoryOperation_AccessManagementService.Controllers.NotificationController
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
        /*
                [HttpGet("getUnread-Notification")]
                public async Task<IActionResult> GetUnread(int tenantId)
                {
                    var data = await _notificationService.GetUnreadNotificationsAsync(tenantId);
                    return Ok(data);
                }*/

        [HttpGet("getUnread-Notification")]
        public async Task<IActionResult> GetUnread(int tenantId, int? userId)
        {
            var data = await _notificationService.GetUnreadNotificationsAsync(tenantId, userId);
            return Ok(data);
        }
        [HttpGet("get-user-incomming-notifications")]
        public async Task<IActionResult> GetUserIncomingNotifications(int tenantId, int? userId)
        {
            var data = await _notificationService.GetUserIncomingNotifications(tenantId, userId);
            {
                return Ok(data);
            }
        }
        [HttpGet("get-user-outgoing-notifications")]
        public async Task<IActionResult> GetUserOutgoingNotifications(int tenantId, int? userId)
        {
            var data = await _notificationService.GetUserOutgoingNotifications(tenantId, userId);
            {
                return Ok(data);
            }
        }
        [HttpGet("get-userNotification")]
        public async Task<IActionResult> GetUserNotifications(int tenantId, int? userId)
        {
            var data = await _notificationService.GetUserNotificationsAsync(tenantId, userId);
            return Ok(data);
        }

        [HttpPost("mark-read-notification")]
        public async Task<IActionResult> MarkNotificationAsReadAsync(int tenantId,
            int? userId,
            int notificationId,
            string type)
        {
            var success = await _notificationService.MarkNotificationAsReadAsync(tenantId, userId, notificationId, type);
            return success ? Ok("Marked as read") : NotFound("Notification not found");
        }
        [HttpPost("mark-all-read-notification")]
        public async Task<IActionResult> MarkAllNotificationsAsReadAsync(int? WorkorderId, int? ServiceRequestId, int? userId, int tenantId, int notificationId)
        {
            var success = await _notificationService.MarkAllNotificationsAsReadAsync(WorkorderId, ServiceRequestId, userId, tenantId, notificationId);
            return success ? Ok("Marked notifications as read") : NotFound("Notifications not found");
        }

        [HttpPost("mark-all-read-notifications")]
        public async Task<IActionResult> MarkAllNotificationsAsReadAsync(
            int tenantId,
            int? userId,
            string type 
        )
        {
            var success = await _notificationService.MarkAllNotificationsAsReadAsync(tenantId, userId, type);
            return success ? Ok("All notifications marked as read") : NotFound("No notifications found");
        }
    }
}
