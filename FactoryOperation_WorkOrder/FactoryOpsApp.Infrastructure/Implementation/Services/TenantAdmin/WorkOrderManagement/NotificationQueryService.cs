using FactoryOperation_WorkOrder.FactoryOpsApp.Application.Interfaces.Repositories.TenantAdmin.WorkOrderManagement;
using FactoryOperation_WorkOrder.FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.WorkOrderServices;
using FactoryOpsApp.Application.DTOs;

namespace FactoryOperation_WorkOrder.FactoryOpsApp.Infrastructure.Implementation.Services.TenantAdmin.WorkOrderManagement
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
