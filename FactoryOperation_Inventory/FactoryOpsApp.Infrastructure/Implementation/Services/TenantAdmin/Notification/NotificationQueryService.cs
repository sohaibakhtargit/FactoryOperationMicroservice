using FactoryOpsApp.Application.DTOs;
using FactoryOpsApp.Application.Interfaces.Repositories.TenantAdmin.Notification;
using FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.Notification;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FactoryOpsApp.Infrastructure.Service.TenantAdmin.Notification
{
    public class NotificationQueryService : INotificationQueryService
    {
        private readonly INotificationQueryRepository _repository;
        public NotificationQueryService(INotificationQueryRepository repository)
        {
            _repository = repository;
        }
        public Task<List<NotificationDto>> GetAllNotificationsAsync(int tenantId)
        {
            return _repository.GetAllNotificationsAsync(tenantId); ;
        }

        public Task<List<NotificationDto>> GetAllWONotification(int tenantId)
        {
            return _repository.GetAllWONotification(tenantId); ;
        }

        public Task<List<NotificationDto>> GetUnreadNotificationsAsync(int tenantId)
        {
            return _repository.GetUnreadNotificationsAsync(tenantId); ;
        }

        public Task<List<NotificationDto>> GetUserNotificationsAsync(int tenantId, int userId)
        {
            return _repository.GetUserNotificationsAsync(tenantId, userId); ;
        }

        public Task<bool> MarkAsReadAsync(int notificationId, int tenantId)
        {
            return _repository.MarkAsReadAsync(notificationId, tenantId); ;
        }
    }
}
