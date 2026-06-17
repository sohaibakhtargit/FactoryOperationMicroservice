using FactoryOperation_NotificationService.FactoryOpsApp.Application.Interfaces.Kafka_Notification;
using FactoryOperation_NotificationService.FactoryOpsApp.Application.Interfaces.NotificationServices;
using FactoryOperation_NotificationService.FactoryOpsApp.Application.Interfaces.Services.EmailServices;
using FactoryOperation_NotificationService.FactoryOpsApp.Common.Models;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

public class NotificationProcessor : INotificationProcessor
{
    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters =
        {
            new JsonStringEnumConverter(null, true)
        }
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

    public async Task ProcessEventAsync(string topic, byte[] jsonPayload)
    {
        _logger.LogDebug("Processing Kafka message from Topic={Topic}", topic);

        if (jsonPayload == null || jsonPayload.Length == 0)
        {
            _logger.LogWarning("Received empty Kafka payload. Topic={Topic}", topic);
            return;
        }

        KafkaMessageEnvelope? envelope;
        string rawJson = Encoding.UTF8.GetString(jsonPayload);

        try
        {
            envelope = JsonSerializer.Deserialize<KafkaMessageEnvelope>(rawJson, JsonOpts);
            if (envelope == null || envelope.Payload.ValueKind == JsonValueKind.Undefined)
            {
                _logger.LogWarning("Invalid Kafka envelope. Topic={Topic}, Raw={Raw}", topic, rawJson);
                return;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to deserialize Kafka envelope. Topic={Topic}, Raw={Raw}", topic, rawJson);
            return;
        }

        var payload = envelope.Payload;

        var map = payload.Deserialize<Dictionary<string, JsonElement>>(JsonOpts)
                  ?? new Dictionary<string, JsonElement>();

        string eventType = GetEventType(map);

        _logger.LogInformation(
            "Kafka event received. Topic={Topic}, EventType={EventType}, CorrelationId={CorrelationId}",
            topic,
            eventType,
            envelope.CorrelationId ?? "N/A");

        _logger.LogInformation(
            "Processing event {EventType} | CorrelationId={CorrelationId}",
            eventType,
            envelope.CorrelationId);

        try
        {
            switch (eventType)
            {
                case "WorkOrderProgressUpdated":
                    await HandleWorkOrderProgressUpdatedAsync(
                        payload.Deserialize<WorkOrderProgressUpdatedEventDto>(JsonOpts));
                    break;

                case "LowStockError":
                case "LowStockWarning":
                    await HandleLowStockAsync(
                        payload.Deserialize<LowStockEventDto>(JsonOpts));
                    break;

                case "PurchaseRequest":
                    await HandlePurchaseRequestAsync(
                        payload.Deserialize<InventoryEventDto>(JsonOpts));
                    break;

                case "UpdatePurchaseRequest":
                    await HandleUpdatePurchaseRequestAsync(
                        payload.Deserialize<InventoryEventDto>(JsonOpts));
                    break;

                case "AssetLocationStatusChange":
                case "AssetStatusChange":
                    await HandleAssetLocationStatusChangeAsync(
                        payload.Deserialize<AssetEventDto>(JsonOpts));
                    break;

                case "Assigned":
                    await HandleWorkOrderAssignedAsync(
                        payload.Deserialize<WorkOrderAssignedEventDto>(JsonOpts));
                    break;

                case "Updated":
                    await HandleWorkOrderUpdatedAsync(
                        payload.Deserialize<WorkOrderEventDto>(JsonOpts)!);
                    break;

                case "Deleted":
                    await HandleWorkOrderDeletedAsync(
                        payload.Deserialize<WorkOrderEventDto>(JsonOpts));
                    break;
                case "ServiceRequestCreated":
                    await HandleServiceRequestCreatedAsync(
                        payload.Deserialize<ServiceRequestEventDto>(JsonOpts));
                    break;
                case "ServiceRequestApproved":
                    await HandleServiceRequestApprovedAsync(
                        payload.Deserialize<ServiceRequestEventDto>(JsonOpts));
                    break;
                case "ServiceRequestRejected":
                    await HandleServiceRequestRejectedAsync(
                       payload.Deserialize<ServiceRequestEventDto>(JsonOpts)!);
                    break;
                case "ServiceRequestWorkOrderAssigned":
                    await HandleServiceRequestWorkOrderAssignedAsync(
                       payload.Deserialize<ServiceRequestEventDto>(JsonOpts)!);
                    break;
                case "ServiceRequestReopened":
                    await HandleServiceRequestReopenedAsync(
                        payload.Deserialize<ServiceRequestEventDto>(JsonOpts)!);
                    break;
                case "WorkOrderApproved":
                case "WorkOrderRejected":
                    await HandleWorkOrderApproveRejectAsync(
                        payload.Deserialize<WorkOrderEventDto>(JsonOpts)!);
                    break;

                case "Created":
                case "DueReminder":
                case "Overdue":
                case "ApprovalReminder":
                    await HandleWorkOrderAsync(
                        payload.Deserialize<WorkOrderEventDto>(JsonOpts));
                    break;
                default:
                    _logger.LogWarning(
                        "Unknown event type. Topic={Topic}, EventType={EventType}, Raw={Raw}",
                        topic,
                        eventType,
                        rawJson);
                    break;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error processing event. Topic={Topic}, EventType={EventType}, Raw={Raw}",
                topic,
                eventType,
                rawJson);
        }
    }

    private static string GetEventType(Dictionary<string, JsonElement> map)
    {
        if (map.TryGetValue("EventType", out var evt))
            return evt.GetString() ?? "UNKNOWN";

        // Backward compatibility fallback
        if (map.ContainsKey("NewStatus") && map.ContainsKey("UpdatedBy"))
            return "WorkOrderProgressUpdated";

        if (map.ContainsKey("ItemId") && map.ContainsKey("ReorderLevel"))
            return "LowStock";

        if (map.ContainsKey("LocationId"))
            return "AssetStatusChange";

        return "UNKNOWN";
    }


    private async Task HandleWorkOrderAsync(WorkOrderEventDto? evt)
    {
        if (evt == null) return;

        try
        {
            // await _repo.SaveAsync(new NotificationModel
            var notificationId = await _repo.SaveAsync(new NotificationModel
            {
                TenantId = evt.TenantId,
                WorkOrderId = evt.WorkOrderId,
                WorkOrderTypeName = evt.WorkOrderTypeName?.ToString(),
                WorkOrderNumber = evt.WorkOrderNumber,
                EventType = evt.EventType,
                NotificationType = evt.EventType,
                TargetUserId = evt.AssignedToUserId,
                TargetTeamId = evt.AssignedToTeamId,
                TargetUsersIds = evt.TargetUsersIds,
                TargetUsersEmails = evt.TargetEmailAddresses,
                Title = $"Work Order {evt.EventType}",
                incomingNotifications = evt.incomingNotifications,
                outgoingNotifications = evt.outgoingNotifications,
                Message = evt.EventType switch
                {
                    "DueReminder" =>
                        $"Work Order {evt.WorkOrderNumber} is due tomorrow",

                    "Overdue" =>
                        $"Work Order {evt.WorkOrderNumber} is overdue",

                    "ApprovalReminder" =>
                        $"Work Order {evt.WorkOrderNumber} is waiting for approval/rejection",

                    _ =>
                        $"{evt.Title} ({evt.WorkOrderNumber}) Created"
                },
                CreatedAt = DateTime.UtcNow,
                CreatedByUserId = evt.CreatedBy,
                AdditionalDataJson = JsonSerializer.Serialize(evt, JsonOpts)
            });
            evt.NotificationId = notificationId;
            await _dispatcher.DispatchAsync(evt);
            await _emailSender.SendWorkOrderEmailAsync(evt);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling WorkOrder event (WO={WO})", evt.WorkOrderId);
        }
    }
    private async Task HandleLowStockAsync(LowStockEventDto? evt)
    {
        if (evt == null) return;

        // Optional: normalize for DB (good practice)
        var eventType = evt.EventType switch
        {
            "LowStockWarning" => "Warning",
            "LowStockError" => "Error",
            _ => "Updated"
        };

        try
        {
            var notificationId = await _repo.SaveAsync(new NotificationModel
            {
                TenantId = evt.TenantId,
                EventType = eventType,
                ItemCode = evt.ItemCode,
                NotificationType = evt.EventType,
                Title = eventType switch
                {
                    "Warning" => $"Low Stock Warning: {evt.ItemName}",
                    "Error" => $"Low Stock Critical: {evt.ItemName}",
                    _ => $"Stock Update: {evt.ItemName}"
                },

                Message = $"{evt.ItemName} stock alert. Available: {evt.QuantityAvailable}",

                TargetUsersIds = evt.TargetUsersIds,
                incomingNotifications = evt.incomingNotifications,

                CreatedAt = DateTime.UtcNow,
                AdditionalDataJson = JsonSerializer.Serialize(evt, JsonOpts)
            });

            evt.NotificationId = notificationId;

            await _dispatcher.DispatchLowStockAsync(evt);

            await _emailSender.SendInventoryLowStockEmailAsync(evt);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling LowStock event (Item={ItemId})", evt.ItemId);
        }
    }

    private async Task HandleWorkOrderProgressUpdatedAsync(WorkOrderProgressUpdatedEventDto? evt)
    {
        if (evt == null) return;

        var eventType = evt.Status switch
        {
            "InProgress" => "Started",
            "OnHold" => "Paused",
            "Completed" => "Completed",
            _ => "Updated"
        };
        try
        {
            var notificationId = await _repo.SaveAsync(new NotificationModel
            {
                TenantId = evt.TenantId,
                WorkOrderId = evt.WorkOrderId,
                WorkOrderTypeName = evt.WorkOrderTypeName,
                WorkOrderNumber = evt.WorkOrderNumber,
                EventType = eventType,
                NotificationType = evt.EventType,

                Title = $"Progress Update: {evt.Action ?? evt.Status}",
                Message = $"Work Order {evt.WorkOrderNumber} updated to {evt.Status}",

                TargetUsersIds = evt.TargetUsersIds,
                incomingNotifications = evt.incomingNotifications,
                outgoingNotifications = evt.outgoingNotifications,

                CreatedAt = DateTime.UtcNow,
                AdditionalDataJson = JsonSerializer.Serialize(evt, JsonOpts)
            });

            evt.NotificationId = notificationId;

            await _dispatcher.DispatchProgressUpdateAsync(evt);
            await _emailSender.SendWorkOrderProgressEmailAsync(evt);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling WorkOrderProgressUpdated (WO={WO})", evt.WorkOrderId);
        }
    }

    private async Task HandlePurchaseRequestAsync(InventoryEventDto? evt)
    {
        if (evt == null) return;

        try
        {
            //  await _repo.SaveAsync(new NotificationModel
            var notificationId = await _repo.SaveAsync(new NotificationModel
            {
                TenantId = evt.TenantId,
                EventType = evt.EventType,
                NotificationType = evt.EventType,
                Title = $"Purchase Request Created: {evt.InventoryName}",
                Message = $"A purchase request for {evt.InventoryName} has been created.",
                TargetUsersIds = evt.SupervisorUserIds,

                Quantity = evt.Quantity,
                Cost = evt.Cost,
                CreatedAt = DateTime.UtcNow,
                AdditionalDataJson = JsonSerializer.Serialize(evt, JsonOpts)
            });
            evt.NotificationId = notificationId;
            await _dispatcher.DispatchPurchaseRequestAsync(evt);
            await _emailSender.SendPurchaseRequestEmailAsync(evt);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling PurchaseRequest event");
        }
    }

    private async Task HandleUpdatePurchaseRequestAsync(InventoryEventDto? evt)
    {
        if (evt == null) return;

        try
        {
            // await _repo.SaveAsync(new NotificationModel
            var notificationId = await _repo.SaveAsync(new NotificationModel
            {
                TenantId = evt.TenantId,
                EventType = evt.EventType,
                NotificationType = evt.EventType,
                Title = $"Purchase Request Updated: {evt.InventoryName}",
                Message = $"Purchase request for {evt.InventoryName} has been updated.",
                TargetUserId = evt.TargetUserId,
                Quantity = evt.Quantity,
                Cost = evt.Cost,
                CreatedAt = DateTime.UtcNow,
                AdditionalDataJson = JsonSerializer.Serialize(evt, JsonOpts)
            });
            evt.NotificationId = notificationId;
            await _dispatcher.DispatchUpdatePurchaseRequestAsync(evt);
            await _emailSender.SendUpdatePurchaseRequestEmailAsync(evt);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling UpdatePurchaseRequest event");
        }
    }

    private async Task HandleAssetLocationStatusChangeAsync(AssetEventDto? evt)
    {
        if (evt == null) return;

        try
        {
            string title = evt.EventType.Equals("AssetLocationStatusChange", StringComparison.OrdinalIgnoreCase)
                ? $"Asset Location Changed: {evt.AssetName}"
                : $"Asset Status Changed: {evt.AssetName}";

            string message = evt.EventType.Equals("AssetLocationStatusChange", StringComparison.OrdinalIgnoreCase)
                ? $"The asset '{evt.AssetName}' has been moved to location '{evt.LocationName}'."
                : $"The status of asset '{evt.AssetName}' has been changed to '{evt.Status}'.";

            //  await _repo.SaveAsync(new NotificationModel
            var notificationId = await _repo.SaveAsync(new NotificationModel
            {
                TenantId = evt.TenantId,
                EventType = evt.EventType,
                NotificationType = evt.EventType,
                Title = title,
                Message = message,
                LocationName = evt.LocationName,
                TargetUserId = evt.TargetUserId,
                CreatedAt = DateTime.UtcNow,
                AdditionalDataJson = JsonSerializer.Serialize(evt, JsonOpts)
            });
            evt.NotificationId = notificationId;
            await _dispatcher.DispatchAssetLocationStatusChangeAsync(evt);
            await _emailSender.SendEmailAssetLocationStatusAsync(evt);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling AssetLocationStatusChange event");
        }
    }

    private async Task HandleWorkOrderAssignedAsync(WorkOrderAssignedEventDto? evt)
    {
        if (evt == null) return;

        try
        {
            //  await _repo.SaveAsync(new NotificationModel
            var notificationId = await _repo.SaveAsync(new NotificationModel
            {
                TenantId = evt.TenantId,
                WorkOrderId = evt.WorkOrderId,
                WorkOrderTypeName = evt.WorkOrderTypeName?.ToString(),
                WorkOrderNumber = evt.WorkOrderNumber,
                EventType = "Assigned",
                NotificationType = evt.EventType,
                Title = "Work Order Assigned",
                Message = $"Work order {evt.WorkOrderNumber} assigned to user {evt.AssignedToUserId}",
                TargetUserId = evt.AssignedToUserId,
                CreatedAt = DateTime.UtcNow,
                incomingNotifications = evt.incomingNotifications,
                outgoingNotifications = evt.outgoingNotifications,
                TargetUsersIds = evt.TargetUsersIds,
                TargetUsersEmails = evt.TargetUsersEmails,
                AdditionalDataJson = JsonSerializer.Serialize(evt, JsonOpts)
            });
            evt.NotificationId = notificationId;
            await _dispatcher.DispatchAssignedToUserAsync(evt);
            await _emailSender.SendWorkOrderAssignedEmailAsync(evt);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling WorkOrderAssigned event");
        }
    }

    private async Task HandleWorkOrderUpdatedAsync(WorkOrderEventDto evt)
    {
        try
        {

            var notificationId = await _repo.SaveAsync(new NotificationModel
            {
                TenantId = evt.TenantId,
                WorkOrderId = evt.WorkOrderId,
                WorkOrderTypeName = evt.WorkOrderTypeName,
                WorkOrderNumber = evt.WorkOrderNumber,
                EventType = evt.EventType,
                NotificationType = evt.EventType,
                TargetUserId = evt.AssignedToUserId,
                TargetTeamId = evt.AssignedToTeamId,
                TargetUsersIds = evt.TargetUsersIds,
                TargetUsersEmails = evt.TargetEmailAddresses,

                Title = "Work Order Updated",
                Message = $"Work order {evt.WorkOrderNumber} Updated",

                CreatedAt = DateTime.UtcNow,

                AdditionalDataJson = JsonSerializer.Serialize(evt, JsonOpts)
            });

            evt.NotificationId = notificationId;

            await _dispatcher.DispatchAsync(evt);
            await _emailSender.SendWorkOrderEmailAsync(evt);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling Updated event (WO={WO})", evt.WorkOrderId);
        }
    }
    private async Task HandleWorkOrderDeletedAsync(WorkOrderEventDto? evt)
    {
        if (evt == null) return;

        try
        {
            // await _repo.SaveAsync(new NotificationModel
            var notificationId = await _repo.SaveAsync(new NotificationModel
            {
                TenantId = evt.TenantId,
                WorkOrderId = evt.WorkOrderId,
                WorkOrderTypeName = evt.WorkOrderTypeName?.ToString(),
                WorkOrderNumber = evt.WorkOrderNumber,
                EventType = "Deleted",
                NotificationType = evt.EventType,
                Title = "Work Order Deleted",
                Message = $"Work order {evt.WorkOrderNumber} has been deleted by {evt.DeletedBy}",
                TargetUserId = evt.AssignedToUserId,
                TargetUsersIds = evt.TargetUsersIds,
                CreatedAt = DateTime.UtcNow,
                AdditionalDataJson = JsonSerializer.Serialize(evt, JsonOpts)
            });
            evt.NotificationId = notificationId;
            await _dispatcher.DispatchWorkOrderDeletedAsync(evt);
            await _emailSender.SendWorkOrderDeletedEmailAsync(evt);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling WorkOrderDeleted event");
        }
    }

    private async Task HandleServiceRequestCreatedAsync(ServiceRequestEventDto? evt)
    {
        if (evt == null) return;
        var eventType = "Created";
        try
        {
            // await _repo.SaveAsync(new NotificationModel
            var notificationId = await _repo.SaveAsync(new NotificationModel
            {
                TenantId = evt.TenantId!.Value,
                WorkOrderId = evt.WorkOrderId,
                ServiceRequestType = evt.RequestType,
                ServiceRequestId = evt.ServiceRequestId,
                ServiceRequestNumber = evt.ServiceRequestNumber,
                WorkOrderTypeName = evt.WorkOrderTypeName?.ToString(),
                EventType = eventType,
                NotificationType = evt.EventType,
                Title = $"Service Request Created",
                Message = $"{evt.Title} ({evt.ServiceRequestNumber}) has been created",
                CreatedAt = DateTime.UtcNow,
                CreatedByUserId = evt.CreatedBy,
                incomingNotifications = evt.incomingNotifications,
                outgoingNotifications = evt.outgoingNotifications,
                TargetUsersIds = evt.TargetUserIds,
                TargetUsersEmails = evt.TargetEmailAddresses,
                AdditionalDataJson = JsonSerializer.Serialize(evt, JsonOpts)
            });
            evt.NotificationId = notificationId;
            await _dispatcher.DispatchServiceRequestAsync(evt);
            await _emailSender.SendServiceRequestEmailAsync(evt);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling ServiceRequestCreated event (SR={SR})", evt.ServiceRequestId);
        }
    }
    private async Task HandleServiceRequestApprovedAsync(ServiceRequestEventDto? evt)
    {
        if (evt == null) return;
        var eventType = "Approved"; 
        try
        {
            // await _repo.SaveAsync(new NotificationModel
            var notificationId = await _repo.SaveAsync(new NotificationModel
            {
                TenantId = evt.TenantId!.Value,
                ServiceRequestId = evt.ServiceRequestId,
                ServiceRequestNumber = evt.ServiceRequestNumber,
                WorkOrderId = evt.WorkOrderId,
                WorkOrderNumber = evt.WorkOrderNumber,
                WorkOrderTypeName = evt.WorkOrderTypeName?.ToString(),
                EventType = eventType,
                NotificationType = evt.EventType,
                Title = "Service Request Approved",
                Message = $"Your request {evt.Title} has been approved",
             /*   TargetUsersIds = new List<int?> { evt.CreatedBy },*/
                incomingNotifications = evt.incomingNotifications,
                outgoingNotifications = evt.outgoingNotifications,
                TargetUsersIds = evt.TargetUserIds,
                TargetUsersEmails = evt.TargetEmailAddresses,
                CreatedAt = DateTime.UtcNow,
                CreatedByUserId = evt.CreatedBy,
                UpdatedBy = evt.ApprovedBy,
                AdditionalDataJson = JsonSerializer.Serialize(evt, JsonOpts)
            });
            evt.NotificationId = notificationId;
            await _dispatcher.DispatchServiceRequestApprovedAsync(evt);
            await _emailSender.SendServiceRequestApprovedEmailAsync(evt);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling ServiceRequestApproved event (SR={SR})", evt.ServiceRequestId);
        }
    }
    private async Task HandleServiceRequestRejectedAsync(ServiceRequestEventDto evt)
    {
        var eventType = "Rejected";

        var notificationId = await _repo.SaveAsync(new NotificationModel
        {
            TenantId = evt.TenantId!.Value,
            ServiceRequestId = evt.ServiceRequestId,
            ServiceRequestNumber = evt.ServiceRequestNumber,
            WorkOrderTypeName = evt.WorkOrderTypeName,
            WorkOrderNumber = evt.WorkOrderNumber!,

            EventType = eventType,
            NotificationType = evt.EventType,
            Title = "Service Request Rejected",

            Message = $"Service Request #{evt.ServiceRequestNumber} rejected",

            CreatedAt = DateTime.UtcNow,
            CreatedByUserId = evt.CreatedBy,
            UpdatedBy = evt.RejectedBy,
            incomingNotifications = evt.incomingNotifications,
            outgoingNotifications = evt.outgoingNotifications,
            TargetUsersIds = evt.TargetUserIds,
            TargetUsersEmails = evt.TargetUsersEmails,

            AdditionalDataJson = JsonSerializer.Serialize(evt, JsonOpts)
        });

        evt.NotificationId = notificationId;

        await _dispatcher.DispatchServiceRequestRejectedAsync(evt);
        await _emailSender.SendServiceRequestRejectedEmailAsync(evt);
    }

    private async Task HandleServiceRequestWorkOrderAssignedAsync(ServiceRequestEventDto evt)
    {
        if (evt == null) return;
        var eventType = "Assigned";
        //  await _repo.SaveAsync(new NotificationModel
        var notificationId = await _repo.SaveAsync(new NotificationModel
        {
            TenantId = evt.TenantId!.Value,
            WorkOrderId = evt.WorkOrderId,
            ServiceRequestId = evt.ServiceRequestId,
            ServiceRequestNumber = evt.ServiceRequestNumber,
            WorkOrderNumber = evt.WorkOrderNumber,
            WorkOrderTypeName = evt.WorkOrderTypeName,
            EventType = eventType,
            NotificationType = evt.EventType,
            Title = "Work Order Assigned for Service Request",
            Message = $"Work order {evt.WorkOrderNumber} has been created for your service request and assigned to user {evt.AssignedTo}",
            TargetUsersIds = evt.TargetUserIds,
            TargetUsersEmails = evt.TargetEmailAddresses,
            incomingNotifications = evt.incomingNotifications,
            outgoingNotifications = evt.outgoingNotifications,
            CreatedAt = DateTime.UtcNow,
            CreatedByUserId = evt.CreatedBy,
            UpdatedBy = evt.UpdatedBy,
            AdditionalDataJson = JsonSerializer.Serialize(evt, JsonOpts)
        });
        evt.NotificationId = notificationId;
        await _dispatcher.DispatchServiceRequestWorkOrderAssignedAsync(evt);
        await _emailSender.SendServiceRequestWorkOrderAssignedEmailAsync(evt);
    }

    private async Task HandleServiceRequestReopenedAsync(ServiceRequestEventDto evt)
    {
        if (evt == null) return;
        var eventType = "Reopened";
        // await _repo.SaveAsync(new NotificationModel
        var notificationId = await _repo.SaveAsync(new NotificationModel
        {
            TenantId = evt.TenantId!.Value,
            ServiceRequestId = evt.ServiceRequestId,
            WorkOrderId = evt.WorkOrderId,
            WorkOrderNumber = evt.WorkOrderNumber,
            WorkOrderTypeName = evt.WorkOrderTypeName?.ToString(),
            ServiceRequestType = evt.RequestType,
            EventType = eventType,
            NotificationType = evt.EventType,
            Title = "Service Request Reopened",
            Message = $"service request {evt.Title} and {evt.ServiceRequestId} has been reopened.",
            UpdatedBy = evt.UpdatedBy,
            incomingNotifications = evt.incomingNotifications,
            outgoingNotifications = evt.outgoingNotifications,
            TargetUsersIds = evt.TargetUserIds,
            TargetUsersEmails = evt.TargetEmailAddresses,
            CreatedAt = DateTime.UtcNow,
            //UpdatedBy = evt.UpdatedBy,
            AdditionalDataJson = JsonSerializer.Serialize(evt, JsonOpts)
        });
        evt.NotificationId = notificationId;
        await _dispatcher.DispatchServiceRequestReopenedAsync(evt);
        await _emailSender.SendServiceRequestReopenedEmailAsync(evt);
    }

    private async Task HandleWorkOrderApproveRejectAsync(WorkOrderEventDto evt)
    {
        var eventType = evt.EventType switch
        {
            "WorkOrderApproved" => "Approved",
            "WorkOrderRejected" => "Rejected",
            _ => "Updated"
        };

        var notificationId = await _repo.SaveAsync(new NotificationModel
        {
            TenantId = evt.TenantId,
            WorkOrderId = evt.WorkOrderId,
            WorkOrderTypeName = evt.WorkOrderTypeName?.ToString(),
            WorkOrderNumber = evt.WorkOrderNumber,

            EventType = eventType,
            NotificationType = evt.EventType,
            Title = $"Work Order {eventType}",

            Message = $"Work Order {evt.WorkOrderNumber} {eventType.ToLower()}",

            CreatedAt = DateTime.UtcNow,

            TargetUsersIds = evt.incomingNotifications,
            outgoingNotifications = evt.outgoingNotifications,
            incomingNotifications = evt.incomingNotifications,

            AdditionalDataJson = JsonSerializer.Serialize(evt, JsonOpts)
        });

        evt.NotificationId = notificationId;
        evt.EventType = eventType;

        await _dispatcher.DispatchWorkOrderApproveRejectAsync(evt);
        await _emailSender.SendWorkOrderApproveRejectEmailAsync(evt);
    }
}

