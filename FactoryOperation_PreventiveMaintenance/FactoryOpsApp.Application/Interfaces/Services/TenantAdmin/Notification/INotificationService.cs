using FactoryOpsApp.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.Notification
{
    public interface INotificationService
    {
        Task NotifyWorkOrderCreated(WorkOrderNotificationDto notification);
        Task NotifyWorkOrderUpdated(WorkOrderNotificationDto notification);
        Task NotifyWorkOrderDeleted(WorkOrderNotificationDto notification);
        Task NotifyWorkOrderStatusChanged(WorkOrderNotificationDto notification);
        Task NotifyUser(int userId, WorkOrderNotificationDto notification);
        Task NotifyTenant(int tenantId, WorkOrderNotificationDto notification);

        Task NotifyWorkOrderAssignmentAsync(WorkOrderNotificationDto notification);

        Task NotifyLowStockAsync(InventoryNotificationDto notification);
    }
}
