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

        /* public Task<List<NotificationDto>> GetUnreadNotificationsAsync(int tenantId)
         {
             return _repository.GetUnreadNotificationsAsync(tenantId); 
         }*/

        public Task<List<NotificationDto>> GetUnreadNotificationsAsync(int tenantId, int userId)
        {
            return _repository.GetUnreadNotificationsAsync(tenantId, userId);
        }


        public Task<List<NotificationDto>> GetUserNotificationsAsync(int tenantId, int userId)
        {
            return _repository.GetUserNotificationsAsync(tenantId, userId); ;
        }

        /*public Task<bool> MarkAsReadAsync(int notificationId, int tenantId)
        {
            return _repository.MarkAsReadAsync(notificationId, tenantId); ;
        }*/
        public Task<bool> MarkNotificationAsReadAsync(int notificationId, int userId, int tenantId)
        {
            return _repository.MarkNotificationAsReadAsync(notificationId, userId, tenantId);
        }
    }
}
