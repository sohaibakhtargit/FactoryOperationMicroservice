using FactoryOperation_NotificationService.FactoryOpsApp.Common.Models;

namespace FactoryOperation_NotificationService.FactoryOpsApp.Application.Interfaces.NotificationServices
{
    public interface INotificationDispatcher
    {
        Task DispatchAsync(WorkOrderEventDto evt);
        Task DispatchLowStockAsync(LowStockEventDto evt);
        Task DispatchProgressUpdateAsync(WorkOrderProgressUpdatedEventDto evt);
        Task DispatchAssignedToUserAsync(WorkOrderAssignedEventDto evt);
        Task DispatchPurchaseRequestAsync(InventoryEventDto evt);
        Task DispatchUpdatePurchaseRequestAsync(InventoryEventDto evt);
        Task DispatchAssetLocationStatusChangeAsync(AssetEventDto evt);

        Task DispatchWorkOrderDeletedAsync(WorkOrderEventDto evt);
        Task DispatchServiceRequestAsync(ServiceRequestEventDto evt);
        Task DispatchServiceRequestApprovedAsync(ServiceRequestEventDto evt);
        Task DispatchServiceRequestRejectedAsync(ServiceRequestEventDto evt);
        Task DispatchServiceRequestWorkOrderAssignedAsync(ServiceRequestEventDto evt);
        Task DispatchServiceRequestReopenedAsync(ServiceRequestEventDto evt);
        Task DispatchWorkOrderApproveRejectAsync(WorkOrderEventDto evt);
    }
}
