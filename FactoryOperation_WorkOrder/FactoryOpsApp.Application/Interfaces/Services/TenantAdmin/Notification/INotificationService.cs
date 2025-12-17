using FactoryOpsApp.Application.DTOs;

namespace FactoryOperation_WorkOrder.FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.Notification
{
    public interface INotificationService
    {
        Task NotifyWorkOrderCreated(WorkOrderNotificationDto notification);
        Task NotifyWorkOrderUpdated(WorkOrderNotificationDto notification);
        Task NotifyWorkOrderDeleted(WorkOrderNotificationDto notification);
        Task NotifyWorkOrderStatusChanged(WorkOrderNotificationDto notification);
        Task NotifyUser(int userId, WorkOrderNotificationDto notification);
        Task NotifyTenant(int tenantId, WorkOrderNotificationDto notification);

        //Assigneee Notification
        Task NotifyWorkOrderAssignmentAsync(WorkOrderNotificationDto notification);

        //Notification-LowStockAsync
        Task NotifyLowStockAsync(InventoryNotificationDto notification);
    }
}
