using FactoryOperation_NotificationService.FactoryOpsApp.Application.Interfaces.NotificationServices;
using FactoryOperation_NotificationService.FactoryOpsApp.Common.Models;
using FactoryOps.NotificationService.Infrastructure.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace FactoryOperation_NotificationService.FactoryOpsApp.Infrastructure.Implementation.Services.NotificationServices
{
    public sealed class NotificationDispatcher : INotificationDispatcher
    {
        private readonly IHubContext<NotificationHub> _hub;
        private readonly ILogger<NotificationDispatcher> _logger;

        public NotificationDispatcher(IHubContext<NotificationHub> hub, ILogger<NotificationDispatcher> logger)
        {
            _hub = hub;
            _logger = logger;
        }

        public async Task DispatchAsync(WorkOrderEventDto evt)
        {
            var tasks = new List<Task>();

            string tenantGroup = $"tenant-{evt.TenantId}";
            string methodName = $"WorkOrder{evt.EventType}";
            // Examples: WorkOrderCreated, WorkOrderUpdated, WorkOrderCompleted

            try
            {
                tasks.Add(
                    _hub.Clients.Group(tenantGroup)
                    .SendAsync(methodName, evt)
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Failed sending tenant group notification (Tenant={Tenant})", evt.TenantId);
            }



            if (evt.AssignedToUserId.HasValue)
            {
                try
                {
                    tasks.Add(
                        _hub.Clients.User(evt.AssignedToUserId.Value.ToString())
                        .SendAsync("IncomingWorkOrderNotification", evt)
                    /* tasks.Add
                         (_hub.Clients.User(evt.AssignedToUserId.Value.ToString())
                          .SendAsync("LowStockNotification", evt)*/
                    );


                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ Failed sending user notification (UserId={UserId})", evt.AssignedToUserId);
                }
            }


            if (evt.AssignedToTeamId.HasValue)
            {
                try
                {
                    string teamGroup = $"team-{evt.AssignedToTeamId.Value}";
                    tasks.Add(
                        _hub.Clients.Group(teamGroup)
                        .SendAsync("IncomingWorkOrderNotification", evt)
                    );
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ Failed sending team notification (TeamId={TeamId})", evt.AssignedToTeamId);
                }
            }

            if (evt.SupervisorUserIds != null)
            {
                foreach (var userIdNullable in evt.SupervisorUserIds.Where(u => u.HasValue))
                {
                    int userId = userIdNullable!.Value;

                    try
                    {
                        tasks.Add(
                            _hub.Clients.User(userId.ToString())
                                .SendAsync("WorkOrderNotification", evt)
                        );
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex,
                            "❌ Failed sending supervisor notification (SupervisorUserId={UserId})",
                            userId);
                    }
                }

            }

            try
            {
                await Task.WhenAll(tasks);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ One or more SignalR send operations failed.");
            }

            _logger.LogInformation("📢 SignalR notification dispatched successfully (WorkOrderId={WO})", evt.WorkOrderId);
        }


        public async Task DispatchAssignedToUserAsync(WorkOrderAssignedEventDto evt)
        {
            var tasks = new List<Task>();

          //  string tenantGroup = $"tenant-{evt.TenantId}";

            try
            {
              /*  if (evt.AssignedToUserId.HasValue)
                {
                    tasks.Add(_hub.Clients.User(evt.AssignedToUserId.Value.ToString())
                        .SendAsync("WorkOrderAssignedNotification", evt));
                }*/
                if (evt.SupervisorUserIds != null)
                {
                    foreach (var userId in evt.SupervisorUserIds)
                    {
                        if (userId.HasValue)
                        {
                            tasks.Add(_hub.Clients.User(userId.Value.ToString())
                                .SendAsync("PurchaseRequisitionCreated", evt));
                        }
                    }
                }
                await Task.WhenAll(tasks);

                _logger.LogInformation($"Notification sent to user: user-{evt.AssignedToUserId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send notification to user {evt.AssignedToUserId}");
            }

        }

        public async Task DispatchLowStockAsync(LowStockEventDto evt)
        {
            var tasks = new List<Task>();

            if (evt.TargetUserIds !=null)
                tasks.Add(_hub.Clients.User(evt.TargetUserIds.ToString())
                    .SendAsync("LowStockNotification", evt));
           

            await Task.WhenAll(tasks);
        }

        public async Task DispatchPurchaseRequestAsync(InventoryEventDto evt)
        {
            var tasks = new List<Task>();
            if (evt.TargetUserIds != null)
            {
                foreach (var userId in evt.TargetUserIds)
                {
                    if (userId.HasValue)
                    {
                        tasks.Add(_hub.Clients.User(userId.Value.ToString())
                            .SendAsync("PurchaseRequisitionCreated", evt));
                    }
                }
            }
        }

        public async Task DispatchUpdatePurchaseRequestAsync(InventoryEventDto evt)
        {
            var tasks = new List<Task>();
            if(evt.TargetUserIds != null)
            {
                tasks.Add(_hub.Clients.User(evt.TargetUserId.ToString())
                    .SendAsync("UpdatePurchaseRequest", evt));
            }
        }
            public async Task DispatchProgressUpdateAsync(WorkOrderProgressUpdatedEventDto evt)
        {
            var tasks = new List<Task>();

            string tenantGroup = $"tenant-{evt.TenantId}";
            if (evt.TargetUserIds != null)
            {
                foreach (var userId in evt.TargetUserIds)
                {
                    if (userId.HasValue)
                    {
                        tasks.Add(_hub.Clients.User(userId.Value.ToString())
                            .SendAsync("WorkOrderProgressUpdated", evt));
                    }
                }
            }

            await Task.WhenAll(tasks);

            _logger.LogInformation("Progress Update SignalR Sent To Supervisors (WO={WO})", evt.WorkOrderId);
        }


    }
}
