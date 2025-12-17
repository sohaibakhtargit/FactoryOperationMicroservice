using FactoryOpsApp.Application.DTOs;

namespace FactoryOperation_WorkOrder.FactoryOpsApp.Application.Interfaces.Repositories.TenantAdmin.WorkOrderManagement
{
    public interface INotificationQueryRepository
    {
        Task<List<NotificationDto>> GetAllNotificationsAsync(int tenantId);
        Task<List<NotificationDto>> GetAllWONotification(int tenantId);
        Task<List<NotificationDto>> GetUnreadNotificationsAsync(int tenantId);
        Task<List<NotificationDto>> GetUserNotificationsAsync(int tenantId, int userId);
        Task<bool> MarkAsReadAsync(int notificationId, int tenantId);
    }
}
