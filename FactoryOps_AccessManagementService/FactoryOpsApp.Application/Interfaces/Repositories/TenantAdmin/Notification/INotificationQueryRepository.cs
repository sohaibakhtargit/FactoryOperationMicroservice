using FactoryOpsApp.Application.DTOs;

namespace FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Repositories.TenantAdmin.Notification
{
    public interface INotificationQueryRepository
    {
        Task<List<NotificationDto>> GetAllNotificationsAsync(int tenantId);
        Task<List<NotificationDto>> GetAllWONotification(int tenantId);
        /*  Task<List<NotificationDto>> GetUnreadNotificationsAsync(int tenantId);*/
        Task<List<NotificationDto>> GetUnreadNotificationsAsync(int tenantId, int userId);
        Task<List<NotificationDto>> GetUserNotificationsAsync(int tenantId, int userId);
        /*  Task<bool> MarkAsReadAsync(int notificationId, int tenantId);*/
        Task<bool> MarkNotificationAsReadAsync(int notificationId, int userId, int tenantId);
    }
}
