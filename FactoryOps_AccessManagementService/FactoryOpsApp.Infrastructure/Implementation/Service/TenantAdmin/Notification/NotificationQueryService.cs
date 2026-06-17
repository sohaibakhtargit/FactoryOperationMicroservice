using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Repositories.TenantAdmin.Notification;
using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.Notification;
using FactoryOpsApp.Application.DTOs;

namespace FactoryOperation_AccessManagementService.FactoryOpsApp.Infrastructure.Implementation.Service.TenantAdmin.Notification
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
            return _repository.GetAllNotificationsAsync(tenantId);
        }

        public Task<List<NotificationDto>> GetAllWONotification(int tenantId)
        {
            return _repository.GetAllWONotification(tenantId);
        }

        public Task<List<NotificationDto>> GetUnreadNotificationsAsync(int tenantId, int? userId)
        {
            return _repository.GetUnreadNotificationsAsync(tenantId, userId);
        }

        public Task<List<NotificationDto>> GetUserIncomingNotifications(int tenantId, int? userId)
        {
            return _repository.GetUserIncomingNotifications(tenantId, userId);
        }

        public Task<List<NotificationDto>> GetUserOutgoingNotifications(int tenantId, int? userId)
        {
            return _repository.GetUserOutgoingNotifications(tenantId, userId);
        }
        public Task<List<NotificationDto>> GetUserNotificationsAsync(int tenantId, int? userId)
        {
            return _repository.GetUserNotificationsAsync(tenantId, userId);
        }

        //public Task<bool> MarkNotificationAsReadAsync(int notificationId, int userId, int tenantId)
        //{
        //    return _repository.MarkNotificationAsReadAsync(notificationId, userId, tenantId);
        //}

        public Task<bool> MarkNotificationAsReadAsync(int tenantId, int? userId, int notificationId, string type)
        {
            return _repository.MarkNotificationAsReadAsync(tenantId, userId, notificationId, type);
        }
        public Task<bool> MarkAllNotificationsAsReadAsync(int? WorkorderId, int? ServiceRequestId, int? userId, int tenantId, int notificationId)
        {
            return _repository.MarkAllNotificationsAsReadAsync(WorkorderId, ServiceRequestId, userId, tenantId, notificationId);
        }

        public Task<bool> MarkAllNotificationsAsReadAsync(int tenantId, int? userId, string type)
        {
            return _repository.MarkAllNotificationsAsReadAsync(tenantId, userId, type);
        }

        public Task<List<NotificationDto>> GetUserIncomingNotifications(int tenantId, int userId)
        {
            throw new NotImplementedException();
        }
    }
}