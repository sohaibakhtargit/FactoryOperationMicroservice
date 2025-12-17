/*using FactoryOperation_NotificationService.FactoryOpsApp.Application.Interfaces.Kafka_Notification;
using FactoryOperation_NotificationService.FactoryOpsApp.Application.Interfaces.NotificationServices;
using FactoryOperation_NotificationService.FactoryOpsApp.Application.Interfaces.Services.EmailServices;
using FactoryOperation_NotificationService.FactoryOpsApp.Common.Models;
using FactoryOperation_NotificationService.FactoryOpsApp.Application.DTOs;
using System.Text.Json;

public class NotificationProcessor : INotificationProcessor
{
    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly INotificationRepository _repo;
    private readonly INotificationDispatcher _dispatcher;
    private readonly ILogger<NotificationProcessor> _logger;
    private readonly INotificationEmailSender _emailSender;

    public NotificationProcessor(
        INotificationRepository repo,
        INotificationDispatcher dispatcher,
        INotificationEmailSender emailSender,
        ILogger<NotificationProcessor> logger)
    {
        _repo = repo;
        _dispatcher = dispatcher;
        _emailSender = emailSender;
        _logger = logger;
    }

    public async Task ProcessEventAsync(string topic, string jsonPayload)
    {
        _logger.LogDebug("Processing message from Topic={Topic}. RawLength={Len}", topic, jsonPayload?.Length);

        if (string.IsNullOrWhiteSpace(jsonPayload))
        {
            _logger.LogWarning("Received null/empty jsonPayload. Topic={Topic}", topic);
            return;
        }

        KafkaMessageEnvelope? envelope;

        try
        {
            envelope = JsonSerializer.Deserialize<KafkaMessageEnvelope>(jsonPayload, JsonOpts);
            if (envelope?.Payload == null)
            {
                _logger.LogWarning("Envelope or payload null. Topic={Topic}, Raw={Raw}", topic, jsonPayload);
                return;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Deserialization failed. Topic={Topic}, Raw={Raw}", topic, jsonPayload);
            return;
        }

        var payload = envelope.Payload;

        try
        {
        
            var map = payload.Deserialize<Dictionary<string, JsonElement>>(JsonOpts) ?? new Dictionary<string, JsonElement>();
            var keys = string.Join(",", map.Keys);
            _logger.LogDebug("Payload keys detected: {Keys}", keys);

            bool Has(string key) => map.Keys.Any(k => string.Equals(k, key, StringComparison.OrdinalIgnoreCase));

         
            if (Has("NewStatus") && Has("UpdatedBy") && Has("WorkOrderProgressUpdated")|| Has(jsonPayload.EventType = "WorkOrderProgressUpdated"))
            {
                var progress = payload.Deserialize<WorkOrderProgressUpdatedEventDto>(JsonOpts);
                if (progress != null)
                {
                    _logger.LogDebug("Detected WorkOrderProgressUpdatedEventDto for WO={WO}", progress.WorkOrderId);
                    await HandleWorkOrderProgressUpdatedAsync(progress);
                    return;
                }
            }
       

            if (Has("ItemId")&& Has(payload.EventType = "LowStockWarning") || Has(payload.EventType = "LowStockWarning"))
            {
                var lowStock = payload.Deserialize<LowStockEventDto>(JsonOpts);
                if (lowStock != null)
                {
                    _logger.LogDebug("Detected LowStockEventDto for ItemId={ItemId}", lowStock.ItemId);
                    await HandleLowStockAsync(lowStock);
                    return;
                }
            }

            if (Has("WorkOrderId"))
            {
                var workOrder = payload.Deserialize<WorkOrderEventDto>(JsonOpts);
                if (workOrder != null)
                {
                    _logger.LogDebug("Detected WorkOrderEventDto for WO={WO}", workOrder.WorkOrderId);
                    await HandleWorkOrderAsync(workOrder);
                    return;
                }
            }

            _logger.LogWarning("Unknown event type received. Topic={Topic}, Raw={Raw}", topic, jsonPayload);
            return;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing event. Topic={Topic}, Raw={Raw}", topic, jsonPayload);
            return;
        }
    }

    private async Task HandleWorkOrderAsync(WorkOrderEventDto evt)
    {
        try
        {
            var model = new NotificationModel
            {
                TenantId = evt.TenantId,
                WorkOrderId = evt.WorkOrderId,
                EventType = evt.EventType,
                TargetUserId = evt.AssignedToUserId,
                TargetTeamId = evt.AssignedToTeamId,
                Title = $"Work Order {evt.EventType}",
                Message = $"{evt.Title} ({evt.WorkOrderNumber}) was {evt.EventType}",
                CreatedAt = DateTime.UtcNow,
                AdditionalDataJson = JsonSerializer.Serialize(evt, JsonOpts)
            };

            await _repo.SaveAsync(model);
            _logger.LogInformation("Saved notification (WO={WO}, Tenant={Tenant})", evt.WorkOrderId, evt.TenantId);

            await _dispatcher.DispatchAsync(evt);
            _logger.LogInformation("Dispatched notification (WO={WO})", evt.WorkOrderId);

            await _emailSender.SendWorkOrderEmailAsync(evt);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Processing WorkOrder pipeline failed (WO={WO})", evt.WorkOrderId);
        }
    }

    private async Task HandleLowStockAsync(LowStockEventDto evt)
    {
        try
        {
            var model = new NotificationModel
            {
                TenantId = evt.TenantId,
                EventType = evt.EventType,
                Title = $"Low Stock: {evt.ItemName}",
                Message = $"{evt.ItemName} is low. Available: {evt.QuantityAvailable}, Reorder Level: {evt.ReorderLevel}",
                TargetUserIds = evt.TargetUserIds,
                TargetTeamId = evt.TargetTeamId,
                CreatedAt = DateTime.UtcNow,
                AdditionalDataJson = JsonSerializer.Serialize(evt, JsonOpts)
            };

            await _repo.SaveAsync(model);
            _logger.LogInformation("Saved low-stock notification (Item={ItemId}, Tenant={Tenant})", evt.ItemId, evt.TenantId);

            await _dispatcher.DispatchLowStockAsync(evt);
            _logger.LogInformation("Dispatched low-stock notifications (Item={ItemId})", evt.ItemId);

            await _emailSender.SendInventoryLowStockEmailAsync(evt);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Processing LowStock pipeline failed (Item={ItemId})", evt.ItemId);
        }
    }

    private async Task HandleWorkOrderProgressUpdatedAsync(WorkOrderProgressUpdatedEventDto evt)
    {
        try
        {
            var model = new NotificationModel
            {
                TenantId = evt.TenantId,
                WorkOrderId = evt.WorkOrderId,
                EventType = "WorkOrderProgressUpdated",
                Title = $"Progress Update: {evt.Action}",
                Message = $"Work Order {evt.WorkOrderNumber} is now {evt.NewStatus}",
                TargetUserId = null,
                CreatedAt = DateTime.UtcNow,
                AdditionalDataJson = JsonSerializer.Serialize(evt, JsonOpts)
            };

            await _repo.SaveAsync(model);

            await _dispatcher.DispatchProgressUpdateAsync(evt);

            await _emailSender.SendWorkOrderProgressEmailAsync(evt);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "WorkOrderProgressUpdated handling failed (WO={WO})", evt.WorkOrderId);
        }
    }
}


*/


using FactoryOperation_NotificationService.FactoryOpsApp.Application.Interfaces.Kafka_Notification;
using FactoryOperation_NotificationService.FactoryOpsApp.Application.Interfaces.NotificationServices;
using FactoryOperation_NotificationService.FactoryOpsApp.Application.Interfaces.Services.EmailServices;
using FactoryOperation_NotificationService.FactoryOpsApp.Common.Models;
using FactoryOperation_NotificationService.FactoryOpsApp.Application.DTOs;
using System.Text.Json;

public class NotificationProcessor : INotificationProcessor
{
    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly INotificationRepository _repo;
    private readonly INotificationDispatcher _dispatcher;
    private readonly ILogger<NotificationProcessor> _logger;
    private readonly INotificationEmailSender _emailSender;

    public NotificationProcessor(
        INotificationRepository repo,
        INotificationDispatcher dispatcher,
        INotificationEmailSender emailSender,
        ILogger<NotificationProcessor> logger)
    {
        _repo = repo;
        _dispatcher = dispatcher;
        _emailSender = emailSender;
        _logger = logger;
    }

    public async Task ProcessEventAsync(string topic, string jsonPayload)
    {
        _logger.LogDebug("Processing Kafka Message from Topic={Topic}", topic);

        if (string.IsNullOrWhiteSpace(jsonPayload))
        {
            _logger.LogWarning("Received empty payload from Kafka.");
            return;
        }

        KafkaMessageEnvelope? envelope;

        try
        {
            envelope = JsonSerializer.Deserialize<KafkaMessageEnvelope>(jsonPayload, JsonOpts);
            if (envelope?.Payload.ValueKind == JsonValueKind.Undefined)
            {
                _logger.LogWarning("Kafka envelope payload is null or invalid.");
                return;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deserializing Kafka envelope");
            return;
        }

        JsonElement payload = envelope.Payload;

        var map = payload.Deserialize<Dictionary<string, JsonElement>>(JsonOpts)
                  ?? new Dictionary<string, JsonElement>();

        bool Has(string key) =>
            map.Keys.Any(k => string.Equals(k, key, StringComparison.OrdinalIgnoreCase));

        try
        {
           
            if (Has("NewStatus") && Has("UpdatedBy") && Has("Action"))
            {
                var progress = payload.Deserialize<WorkOrderProgressUpdatedEventDto>(JsonOpts);
                if (progress != null)
                {
                    _logger.LogInformation("Detected WorkOrderProgressUpdated event.");
                    await HandleWorkOrderProgressUpdatedAsync(progress);
                    return;
                }
            }

            if (Has("ItemId") && Has("ReorderLevel"))
            {
                var lowStock = payload.Deserialize<LowStockEventDto>(JsonOpts);
                if (lowStock != null)
                {
                    _logger.LogInformation("Detected LowStock event for Item={ItemId}", lowStock.ItemId);
                    await HandleLowStockAsync(lowStock);
                    return;
                }
            }
            if (Has("InventoryId") && Has("EventType"))
            {
                string eventType = map["EventType"].GetString() ?? "";
                if (eventType.Equals("PurchaseRequest", StringComparison.OrdinalIgnoreCase))
                {
                    var purchaseReq = payload.Deserialize<InventoryEventDto>(JsonOpts);
                    if (purchaseReq != null)
                    {
                        _logger.LogInformation("Detected PurchaseRequest event.");
                        await HandlePurchaseRequestAsync(purchaseReq);
                        return;
                    }
                }
            }

                if (Has("WorkOrderId") && Has("EventType"))
                {
                    string eventType = map["EventType"].GetString() ?? "";

                    if (eventType.Equals("Assigned", StringComparison.OrdinalIgnoreCase))
                    {
                        var assignEvt = payload.Deserialize<WorkOrderAssignedEventDto>(JsonOpts);
                        if (assignEvt != null)
                        {
                            _logger.LogInformation("Detected WorkOrderAssigned event.");
                            await HandleWorkOrderAssignedAsync(assignEvt);
                            return;
                        }
                    }

                    var workOrder = payload.Deserialize<WorkOrderEventDto>(JsonOpts);
                    if (workOrder != null)
                    {
                        _logger.LogInformation("Detected WorkOrder event type={Type}", workOrder.EventType);
                        await HandleWorkOrderAsync(workOrder);
                        return;
                    }
                }
    

            _logger.LogWarning("Unknown event received. Raw = {Raw}", jsonPayload);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while processing event.");
        }
    }

    private async Task HandlePurchaseRequestAsync(InventoryEventDto evt)
    {
        try
        {
            await _repo.SaveAsync(new NotificationModel
            {
                TenantId = evt.TenantId,
                EventType = evt.EventType,
                Title = $"Purchase Request Created: {evt.InventoryName}",
                Message = $"A purchase request for {evt.InventoryName} has been created.",
                TargetUserIds = evt.SupervisorUserIds, 
                CreatedAt = DateTime.UtcNow,
                AdditionalDataJson = JsonSerializer.Serialize(evt)
            });

           await _dispatcher.DispatchPurchaseRequestAsync(evt);
            await _emailSender.SendPurchaseRequestEmailAsync(evt);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling PurchaseRequest event.");
        }
    }

    private async Task HandleWorkOrderAsync(WorkOrderEventDto evt)
    {
        try
        {
            await _repo.SaveAsync(new NotificationModel
            {
                TenantId = evt.TenantId,
                WorkOrderId = evt.WorkOrderId,
                EventType = evt.EventType,
                TargetUserId = evt.AssignedToUserId,
                TargetTeamId = evt.AssignedToTeamId,
                TargetUserIds = evt.SupervisorUserIds,
                Title = $"Work Order {evt.EventType}",
                Message = $"{evt.Title} ({evt.WorkOrderNumber}) was {evt.EventType}",
                CreatedAt = DateTime.UtcNow,
                AdditionalDataJson = JsonSerializer.Serialize(evt)
            });

            await _dispatcher.DispatchAsync(evt);
            await _emailSender.SendWorkOrderEmailAsync(evt);

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling WorkOrder event.");
        }
    }

    private async Task HandleWorkOrderAssignedAsync(WorkOrderAssignedEventDto evt)
    {
        try
        {
            await _repo.SaveAsync(new NotificationModel
            {
                TenantId = evt.TenantId,
                WorkOrderId = evt.WorkOrderId,
                EventType = "Assigned",
                Title = "Work Order Assigned",
                Message = $"You have been assigned work order {evt.WorkOrderNumber}",
                TargetUserId = evt.AssignedToUserId,
                CreatedAt = DateTime.UtcNow,
                AdditionalDataJson = JsonSerializer.Serialize(evt)
            });

            await _dispatcher.DispatchAssignedToUserAsync(evt);
            await _emailSender.SendWorkOrderAssignedEmailAsync(evt);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling WorkOrderAssigned event.");
        }
    }


    private async Task HandleLowStockAsync(LowStockEventDto evt)
    {
        try
        {
            await _repo.SaveAsync(new NotificationModel
            {
                TenantId = evt.TenantId,
                EventType = evt.EventType,
                Title = $"Low Stock: {evt.ItemName}",
                Message = $"{evt.ItemName} is low. Available: {evt.QuantityAvailable}",
                TargetUserIds = evt.TargetUserIds,
                CreatedAt = DateTime.UtcNow,
                AdditionalDataJson = JsonSerializer.Serialize(evt)
            });

            await _dispatcher.DispatchLowStockAsync(evt);
            await _emailSender.SendInventoryLowStockEmailAsync(evt);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling LowStock event.");
        }
    }

    private async Task HandleWorkOrderProgressUpdatedAsync(WorkOrderProgressUpdatedEventDto evt)
    {
        try
        {
            await _repo.SaveAsync(new NotificationModel
            {
                TenantId = evt.TenantId,
                WorkOrderId = evt.WorkOrderId,
                EventType = "WorkOrderProgressUpdated",
                Title = $"Progress Update: {evt.Action}",
                Message = $"Work Order {evt.WorkOrderNumber} updated to {evt.NewStatus}",
                CreatedAt = DateTime.UtcNow,
                AdditionalDataJson = JsonSerializer.Serialize(evt)
            });

            await _dispatcher.DispatchProgressUpdateAsync(evt);
            await _emailSender.SendWorkOrderProgressEmailAsync(evt);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling WorkOrderProgressUpdated event.");
        }
    }
}
