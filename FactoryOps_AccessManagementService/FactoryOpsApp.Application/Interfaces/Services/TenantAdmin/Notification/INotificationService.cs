using FactoryOpsApp.Application.DTOs;
using FactoryOpsApp.Domain.Entities;

namespace FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.Notification
{
    public interface INotificationService
    {
        Task NotifyWorkOrderCreatedToManagers(WorkOrderNotificationDto notification, List<int> managerUserIds);
        Task NotifyTenant1(int tenantId, WorkOrderNotificationDto notification);
        Task NotifyWorkOrderCreated(WorkOrderNotificationDto notification);
        Task NotifyWorkOrderUpdated(WorkOrderNotificationDto notification);
        Task NotifyWorkOrderDeleted(WorkOrderNotificationDto notification);
        Task NotifyWorkOrderStatusChanged(WorkOrderNotificationDto notification);
        Task NotifyUser(int userId, WorkOrderNotificationDto notification);
        Task NotifyUserMulti(int userId, IEnumerable<int?> userIds, WorkOrderNotificationDto notification);
        Task NotifyTenant(int tenantId, WorkOrderNotificationDto notification);
        //Assigneee Notification
        Task NotifyWorkOrderAssignmentAsync(WorkOrderNotificationDto notification);
        Task NotifyAssignedUser(int userId, WorkOrderNotificationDto notification);
        Task NotifyLowStockAlert(List<int> userId, MasterNotification notification);
        Task NotifyWorkOrderUpdateAsync(int userId, WorkOrderNotificationDto notification);
        Task NotifyWorkOrderUpdateAsyncSupervisor(List<int> userId, WorkOrderNotificationDto notification);
        //Notification-LowStockAsync
        Task NotifyLowStockAsync(InventoryNotificationDto notification);
    }
}
