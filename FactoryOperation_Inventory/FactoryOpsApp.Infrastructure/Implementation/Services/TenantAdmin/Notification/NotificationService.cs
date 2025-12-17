using FactoryOpsApp.Application.DTOs;
using FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.Notification;
using FactoryOpsApp.Infrastructure.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace FactoryOpsApp.Infrastructure.Service.TenantAdmin.Notification
{
    public class NotificationService : INotificationService
    {
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(
            IHubContext<NotificationHub> hubContext,
            ILogger<NotificationService> logger)
        {
            _hubContext = hubContext;
            _logger = logger;
        }

        #region === Work Order Created ===
        public async Task NotifyWorkOrderCreated(WorkOrderNotificationDto notification)
        {
            try
            {
                // Notify all users in the tenant
                await _hubContext.Clients.Group($"tenant-{notification.TenantId}")
                    .SendAsync("WorkOrderCreated", notification);

                //if (notification.AssignedToUserId.HasValue)
                //{
                //    await _hubContext.Clients.User(notification.AssignedToUserId.Value.ToString())
                //        .SendAsync("WorkOrderAssigned", notification);
                //}

                _logger.LogInformation($"✅ Work order created notification sent for {notification.WorkOrderNumber}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Failed to send work order created notification");
            }
        }
        #endregion

        #region === Work Order Updated ===
        public async Task NotifyWorkOrderUpdated(WorkOrderNotificationDto notification)
        {
            try
            {
                await _hubContext.Clients.Group($"tenant-{notification.TenantId}")
                    .SendAsync("WorkOrderUpdated", notification);

                _logger.LogInformation($"✅ Work order updated notification sent for {notification.WorkOrderNumber}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Failed to send work order updated notification");
            }
        }
        #endregion

        #region === Work Order Deleted ===
        public async Task NotifyWorkOrderDeleted(WorkOrderNotificationDto notification)
        {
            try
            {
                await _hubContext.Clients.Group($"tenant-{notification.TenantId}")
                    .SendAsync("WorkOrderDeleted", notification);

                _logger.LogInformation($"Work order deleted notification sent for {notification.WorkOrderNumber}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send work order deleted notification");
            }
        }
        #endregion

        #region === Work Order Status Changed ===
        public async Task NotifyWorkOrderStatusChanged(WorkOrderNotificationDto notification)
        {
            try
            {
                // Notify all users in the tenant
                await _hubContext.Clients.Group($"tenant-{notification.TenantId}")
                    .SendAsync("WorkOrderStatusChanged", notification);

                // Notify the assigned user about the change
                if (notification.AssignedToUserId.HasValue)
                {
                    await _hubContext.Clients.User(notification.AssignedToUserId.Value.ToString())
                        .SendAsync("WorkOrderStatusChanged", notification);
                }

                _logger.LogInformation($"Work order status change notification sent for {notification.WorkOrderNumber}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send work order status changed notification");
            }
        }
        #endregion

        #region === Notify Specific User ===
        public async Task NotifyUser(int userId, WorkOrderNotificationDto notification)
        {
            try
            {
                await _hubContext.Clients.User(userId.ToString())
                    .SendAsync("PersonalNotification", notification);

                _logger.LogInformation($"Personal notification sent to user: {userId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send personal notification to user {userId}");
            }
        }
        #endregion

        #region === Notify Tenant ===
        public async Task NotifyTenant(int tenantId, WorkOrderNotificationDto notification)
        {
            try
            {
                await _hubContext.Clients.Group($"tenant-{tenantId}")
                    .SendAsync("TenantNotification", notification);

                _logger.LogInformation($"Tenant notification sent for tenant: {tenantId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send tenant notification for tenant {tenantId}");
            }
        }
        #endregion

        #region === Unified Incoming/Outgoing Notification ===
        public async Task NotifyWorkOrderAssignmentAsync(WorkOrderNotificationDto notification)
        {
            try
            {
                if (notification.CreatedByUserId.HasValue)
                {
                    await _hubContext.Clients.User(notification.CreatedByUserId.Value.ToString())
                        .SendAsync("OutgoingWorkOrderNotification", new
                        {
                            notification.WorkOrderId,
                            notification.WorkOrderNumber,
                            notification.Title,
                            notification.Message,
                            Type = "Outgoing",
                            notification.EventTime
                        });
                }

                // Incoming → Assigned Technician
                if (notification.AssignedToUserId.HasValue)
                {
                    await _hubContext.Clients.User(notification.AssignedToUserId.Value.ToString())
                        .SendAsync("IncomingWorkOrderNotification", new
                        {
                            notification.WorkOrderId,
                            notification.WorkOrderNumber,
                            notification.Title,
                            notification.Message,
                            Type = "Incoming",
                            notification.EventTime
                        });
                }

                // Incoming → Assigned Team
                if (notification.AssignedToTeamId.HasValue)
                {
                    await _hubContext.Clients.Group($"team-{notification.AssignedToTeamId.Value}")
                        .SendAsync("IncomingWorkOrderNotification", new
                        {
                            notification.WorkOrderId,
                            notification.WorkOrderNumber,
                            notification.Title,
                            notification.Message,
                            Type = "Incoming",
                            notification.EventTime
                        });
                }

                _logger.LogInformation($"Work order (incoming/outgoing) notification sent for {notification.WorkOrderNumber}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send incoming/outgoing notification for {notification.WorkOrderNumber}");
            }
        }
        #endregion

        #region
        public async Task NotifyLowStockAsync(InventoryNotificationDto notification)
        {
            try
            {
                if (notification.TargetUserId.HasValue)
                {
                    await _hubContext.Clients
                        .User(notification.TargetUserId.Value.ToString())
                        .SendAsync("LowStockNotification", new
                        {
                            notification.ItemId,
                            notification.ItemName,
                            notification.QuantityAvailable,
                            notification.ReorderLevel,
                            notification.Title,
                            notification.Message,
                            notification.EventType,
                            notification.EventTime
                        });
                }

                await _hubContext.Clients
                    .Group($"tenant-{notification.TenantId}")
                    .SendAsync("LowStockBroadcast", notification);

                _logger.LogInformation($"Low stock notification sent for {notification.ItemName} (Qty={notification.QuantityAvailable})");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send low stock notification for {notification.ItemName}");
            }
        }

        #endregion
    }
}
