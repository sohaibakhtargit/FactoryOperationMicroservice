using FactoryOperation_NotificationService.FactoryOpsApp.Common.Models;

namespace FactoryOperation_NotificationService.FactoryOpsApp.Application.Interfaces.Services.EmailServices
{
    public interface INotificationEmailSender
    {
        Task SendWorkOrderEmailAsync(WorkOrderEventDto evt);
        Task SendInventoryLowStockEmailAsync(LowStockEventDto evt);
        Task SendWorkOrderProgressEmailAsync(WorkOrderProgressUpdatedEventDto evt);
        Task SendWorkOrderAssignedEmailAsync(WorkOrderAssignedEventDto evt);
        Task SendPurchaseRequestEmailAsync(InventoryEventDto evt);
    }
}
