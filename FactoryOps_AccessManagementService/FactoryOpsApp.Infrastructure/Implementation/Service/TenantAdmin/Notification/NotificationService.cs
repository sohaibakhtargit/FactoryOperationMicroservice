using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.Notification;
using FactoryOpsApp.Application.DTOs;
using FactoryOpsApp.Domain.Entities;
using Microsoft.AspNetCore.SignalR;

namespace FactoryOperation_AccessManagementService.FactoryOpsApp.Infrastructure.Implementation.Service.TenantAdmin.Notification
{
    public class NotificationService : INotificationService
    {
        public Task NotifyAssignedUser(int userId, WorkOrderNotificationDto notification)
        {
            throw new NotImplementedException();
        }

        public Task NotifyLowStockAlert(List<int> userId, MasterNotification notification)
        {
            throw new NotImplementedException();
        }

        public Task NotifyLowStockAsync(InventoryNotificationDto notification)
        {
            throw new NotImplementedException();
        }

        public Task NotifyTenant(int tenantId, WorkOrderNotificationDto notification)
        {
            throw new NotImplementedException();
        }

        public Task NotifyTenant1(int tenantId, WorkOrderNotificationDto notification)
        {
            throw new NotImplementedException();
        }

        public Task NotifyUser(int userId, WorkOrderNotificationDto notification)
        {
            throw new NotImplementedException();
        }

        public Task NotifyUserMulti(int userId, IEnumerable<int?> userIds, WorkOrderNotificationDto notification)
        {
            throw new NotImplementedException();
        }

        public Task NotifyWorkOrderAssignmentAsync(WorkOrderNotificationDto notification)
        {
            throw new NotImplementedException();
        }

        public Task NotifyWorkOrderCreated(WorkOrderNotificationDto notification)
        {
            throw new NotImplementedException();
        }

        public Task NotifyWorkOrderCreatedToManagers(WorkOrderNotificationDto notification, List<int> managerUserIds)
        {
            throw new NotImplementedException();
        }

        public Task NotifyWorkOrderDeleted(WorkOrderNotificationDto notification)
        {
            throw new NotImplementedException();
        }

        public Task NotifyWorkOrderStatusChanged(WorkOrderNotificationDto notification)
        {
            throw new NotImplementedException();
        }

        public Task NotifyWorkOrderUpdateAsync(int userId, WorkOrderNotificationDto notification)
        {
            throw new NotImplementedException();
        }

        public Task NotifyWorkOrderUpdateAsyncSupervisor(List<int> userId, WorkOrderNotificationDto notification)
        {
            throw new NotImplementedException();
        }

        public Task NotifyWorkOrderUpdated(WorkOrderNotificationDto notification)
        {
            throw new NotImplementedException();
        }
    }
}