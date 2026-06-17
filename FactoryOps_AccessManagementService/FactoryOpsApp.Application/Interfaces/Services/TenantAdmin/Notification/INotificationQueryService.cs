using FactoryOpsApp.Application.DTOs;

namespace FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.Notification
{
    public interface INotificationQueryService
    {
        Task<List<NotificationDto>> GetAllNotificationsAsync(int tenantId);
        Task<List<NotificationDto>> GetAllWONotification(int tenantId);
        Task<List<NotificationDto>> GetUnreadNotificationsAsync(int tenantId, int? userId);
        Task<List<NotificationDto>>GetUserIncomingNotifications(int tenantId, int? userId);
        Task<List<NotificationDto>> GetUserOutgoingNotifications(int tenantId, int? userId);

        Task<List<NotificationDto>> GetUserNotificationsAsync(int tenantId, int? userId);
        //Task<bool> MarkNotificationAsReadAsync(int notificationId, int userId, int tenantId);
        Task<bool> MarkNotificationAsReadAsync(
                int tenantId,
                int? userId,
                int notificationId,
                string type // incoming | outgoing | bell
                );
        Task<bool> MarkAllNotificationsAsReadAsync(int? WorkorderId, int? ServiceRequestId, int? userId, int tenantId, int notificationId);
        Task<bool> MarkAllNotificationsAsReadAsync(
            int tenantId,
            int? userId,
            string type
        );

    }
}
