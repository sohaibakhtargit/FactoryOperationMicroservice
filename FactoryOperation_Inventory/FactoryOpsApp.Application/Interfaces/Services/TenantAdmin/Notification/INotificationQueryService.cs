using FactoryOpsApp.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.Notification
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
