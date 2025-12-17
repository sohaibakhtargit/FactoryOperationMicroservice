using FactoryOpsApp.Application.DTOs;

namespace FactoryOperation_WorkOrder.FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.WorkOrderServices
{
    public interface INotificationQueryService
    {
        Task<List<NotificationDto>> GetAllNotificationsAsync(int tenantId);
        Task<List<NotificationDto>> GetAllWONotification(int tenantId);
        Task<List<NotificationDto>> GetUnreadNotificationsAsync(int tenantId);
        Task<List<NotificationDto>> GetUserNotificationsAsync(int tenantId, int userId);
        Task<bool> MarkAsReadAsync(int notificationId, int tenantId);
    }
}
