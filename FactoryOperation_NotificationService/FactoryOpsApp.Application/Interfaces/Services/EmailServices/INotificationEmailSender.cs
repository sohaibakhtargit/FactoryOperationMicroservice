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
        Task SendUpdatePurchaseRequestEmailAsync(InventoryEventDto evt);
        Task SendEmailAssetLocationStatusAsync (AssetEventDto evt);
        Task SendWorkOrderDeletedEmailAsync(WorkOrderEventDto evt);
        Task SendServiceRequestEmailAsync(ServiceRequestEventDto evt);
        Task SendServiceRequestApprovedEmailAsync(ServiceRequestEventDto evt);
        Task SendServiceRequestRejectedEmailAsync(ServiceRequestEventDto evt);
        Task SendServiceRequestWorkOrderAssignedEmailAsync(ServiceRequestEventDto evt);
        Task SendServiceRequestReopenedEmailAsync(ServiceRequestEventDto evt);
        Task SendWorkOrderApproveRejectEmailAsync(WorkOrderEventDto evt);
    }
}
