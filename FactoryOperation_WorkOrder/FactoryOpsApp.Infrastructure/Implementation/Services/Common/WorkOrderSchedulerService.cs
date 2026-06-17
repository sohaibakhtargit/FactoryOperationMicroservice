using FactoryOperation_WorkOrder.FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.Common;
using FactoryOperation_WorkOrder.FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.EventTrace;
using FactoryOperation_WorkOrder.FactoryOpsApp.Application.Models;
using FactoryOperation_WorkOrder.FactoryOpsApp.Infrastructure.Implementation.Services.Common;
using FactoryOperation_WorkOrder.FactoryOpsApp.Infrastructure.Implementation.Services.TenantAdmin.WorkOrderManagement;
using FactoryOpsApp.Domain.Entities.FactoryOpsTenants;
using FactoryOpsApp.Infrastructure.DBContext;
using Microsoft.EntityFrameworkCore;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using System.Text;
using System.Text.Json;
using static FactoryOperation_WorkOrder.FactoryOpsApp.Common.CommonConstantURLs;
using static FactoryOpsApp.Application.DTOs.WorkOrderCreateDto;

public class WorkOrderSchedulerService : IWorkOrderSchedulerService
{
    private readonly MasterFactoryOpsDbContext _masterDb;
    private readonly TenantDbContextFactory _tenantFactory;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<WorkOrderSchedulerService> _logger;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly INotificationBuilderService _notificationBuilderService;
    public WorkOrderSchedulerService(
        MasterFactoryOpsDbContext masterDb,
        TenantDbContextFactory tenantFactory,
        IHttpClientFactory httpClientFactory,
        ILogger<WorkOrderSchedulerService> logger,
        IServiceScopeFactory scopeFactory,
        INotificationBuilderService notificationBuilderService)
    {
        _masterDb = masterDb;
        _tenantFactory = tenantFactory;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _scopeFactory = scopeFactory;
        _notificationBuilderService = notificationBuilderService;
    }

    //public async Task ProcessWorkOrdersAsync()
    //{
    //    var tenants = await _masterDb.FactoryTenants
    //        .Where(x => !x.IsDeleted)
    //        .Select(x => x.TenantId)
    //        .ToListAsync();

    //    foreach (var tenantId in tenants)
    //    {
    //        try
    //        {
    //            using var db = _tenantFactory.GetTenantDbContext(tenantId);

    //            var now = DateTime.UtcNow;

    //            // OVERDUE WORK ORDERS
    //            var overdueWorkOrders = await db.WorkOrders
    //                .Where(x =>
    //                    x.DueDate.HasValue &&
    //                    x.DueDate.Value < now &&
    //                    x.Status != WorkOrderStatus.Completed &&
    //                    x.Status != WorkOrderStatus.Overdue &&
    //                    x.Status != WorkOrderStatus.Closed &&
    //                    !x.IsDeleted)
    //                .ToListAsync();

    //            foreach (var wo in overdueWorkOrders)
    //            {
    //                wo.Status = WorkOrderStatus.Overdue;
    //                wo.UpdatedAt = now;

    //                _logger.LogInformation($"[Scheduler] Overdue WO: {wo.WorkOrderNumber}");

    //                await PublishWorkOrderEventAsync(tenantId, wo, "Overdue");
    //            }

    //            // TODAY REMINDER
    //            var today = now.Date;

    //            var todayDueWorkOrders = await db.WorkOrders
    //                .Where(x =>
    //                    x.DueDate.HasValue &&
    //                    x.DueDate.Value.Date == today &&
    //                    !x.IsDeleted &&
    //                    x.Status != WorkOrderStatus.Completed &&
    //                    x.Status != WorkOrderStatus.Closed &&
    //                    x.IsReminderSent == false)
    //                .ToListAsync();

    //            foreach (var wo in todayDueWorkOrders)
    //            {
    //                _logger.LogInformation($"[Scheduler] Due Reminder WO: {wo.WorkOrderNumber}");

    //                await PublishWorkOrderEventAsync(tenantId, wo, "DueReminder");

    //                wo.IsReminderSent = true;
    //                wo.Status = WorkOrderStatus.Overdue;
    //                wo.UpdatedAt = now;
    //            }

    //            await db.SaveChangesAsync();
    //        }
    //        catch (Exception ex)
    //        {
    //            _logger.LogError(ex, $"Scheduler failed for tenant {tenantId}");
    //        }
    //    }
    //}


    public async Task ProcessWorkOrdersAsync()
    {
        var tenants = await _masterDb.FactoryTenants
            .Where(x => !x.IsDeleted)
            .Select(x => x.TenantId)
            .ToListAsync();

        // Use IST instead of UTC
        var now = DateTime.UtcNow;

        var today = now.Date;
        var tomorrow = today.AddDays(1);

        foreach (var tenantId in tenants)
        {
            try
            {
                using var db = _tenantFactory.GetTenantDbContext(tenantId);

                // =========================
                // 1. REMINDER (1 DAY BEFORE)
                // =========================
                var tomorrowStart = tomorrow;
                var tomorrowEnd = tomorrow.AddDays(1);

                var reminderWorkOrders = await db.WorkOrders
                    .Where(x =>
                        x.DueDate.HasValue &&
                        x.DueDate.Value >= tomorrowStart &&
                        x.DueDate.Value < tomorrowEnd &&
                        !x.IsDeleted &&
                        x.Status != WorkOrderStatus.Completed &&
                        x.Status != WorkOrderStatus.Closed &&
                        x.IsReminderSent == false)
                    .ToListAsync();

                foreach (var wo in reminderWorkOrders)
                {
                    _logger.LogInformation($"[Scheduler] Tomorrow Reminder WO: {wo.WorkOrderNumber}");

                    await PublishWorkOrderEventAsync(tenantId, wo, "DueReminder");

                    wo.IsReminderSent = true;
                    wo.UpdatedAt = now;

                    // DO NOT SET OVERDUE HERE
                }

                // =========================
                // 2. OVERDUE (AFTER DUE DATE PASSED)
                // =========================
                var overdueWorkOrders = await db.WorkOrders
                    .Where(x =>
                        x.DueDate.HasValue &&
                        x.DueDate.Value < now &&
                        x.Status != WorkOrderStatus.Completed &&
                        x.Status != WorkOrderStatus.Closed &&
                        x.Status != WorkOrderStatus.Overdue &&
                        !x.IsDeleted)
                    .ToListAsync();

                foreach (var wo in overdueWorkOrders)
                {
                    wo.Status = WorkOrderStatus.Overdue;
                    wo.UpdatedAt = now;

                    _logger.LogInformation($"[Scheduler] Overdue WO: {wo.WorkOrderNumber}");

                    await PublishWorkOrderEventAsync(tenantId, wo, "Overdue");
                }

                // =========================
                // 3. APPROVAL REMINDER
                // =========================

                var approvalReminderWorkOrders = await db.WorkOrders
                    .Where(x =>
                        x.Status == WorkOrderStatus.Completed &&
                        x.CompletedAt.HasValue &&
                        x.CompletedAt.Value.AddHours(24) <= now &&
                        !x.IsApprovalReminderSent &&
                        !x.IsDeleted)
                    .ToListAsync();

                foreach (var wo in approvalReminderWorkOrders)
                {
                    _logger.LogInformation(
                        $"[Scheduler] Approval Reminder WO: {wo.WorkOrderNumber}");

                    await PublishWorkOrderEventAsync(
                        tenantId,
                        wo,
                        "ApprovalReminder");

                    wo.IsApprovalReminderSent = true;
                    wo.UpdatedAt = now;
                }

                await db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Scheduler failed for tenant {tenantId}");
            }
        }
    }

    private async Task PublishWorkOrderEventAsync(int tenantId, WorkOrder wo, string eventType)
    {
        var correlationId = Guid.NewGuid().ToString();

        using var scope = _scopeFactory.CreateScope();
        var _trace = scope.ServiceProvider.GetRequiredService<IEventTraceLogger>();
        using var tenantDb = _tenantFactory.GetTenantDbContext(tenantId);

        /*var targetUsers = new List<int?>();


        if (wo.AssignedToUserId.HasValue)
            targetUsers.Add(wo.AssignedToUserId.Value);

        targetUsers = targetUsers.Distinct().ToList();*/
        //var targetUsers = new List<int?>();
        //if (wo.AssignedToUserId.HasValue)
        //    targetUsers.Add(wo.AssignedToUserId.Value);

        //var maintenanceSupervisorRoleId = await tenantDb.FactoryRoles
        //    .Where(x => x.RoleName == "Maintenance Supervisor" && !x.IsDeleted)
        //    .Select(x => x.RoleId)
        //    .FirstOrDefaultAsync();

        //var productionSupervisorRoleId = await tenantDb.FactoryRoles
        //    .Where(x => x.RoleName == "Production Supervisor" && !x.IsDeleted)
        //    .Select(x => x.RoleId)
        //    .FirstOrDefaultAsync();

        //var maintenanceSupervisorUserIds = await tenantDb.FactoryUserRoles
        //    .Where(x => x.TenantId == tenantId &&
        //                x.RoleId == maintenanceSupervisorRoleId &&
        //                !x.IsDeleted)
        //    .Select(x => x.UserId)
        //    .ToListAsync();

        //var productionSupervisorUserIds = await tenantDb.FactoryUserRoles
        //    .Where(x => x.TenantId == tenantId &&
        //                x.RoleId == productionSupervisorRoleId &&
        //                !x.IsDeleted)
        //    .Select(x => x.UserId)
        //    .ToListAsync();
        //if (wo.AssignedToTeamId.HasValue)
        //{
        //    targetUsers.AddRange(
        //    maintenanceSupervisorUserIds
        //    .Concat(productionSupervisorUserIds)
        //    .Append(wo.AssignedToUserId!.Value)
        //    .Select(x => (int?)x)
        //);
        //}
        //else
        //{
        //    targetUsers.AddRange(
        //   maintenanceSupervisorUserIds
        //   .Concat(productionSupervisorUserIds)
        //   .Select(x => (int?)x));
        //}
     
        //targetUsers = targetUsers.Distinct().ToList();

        var assignedToUserIdAsset = await tenantDb.AssetRegistry
                   .Where(x => x.AssetId == wo.AssetId && x.IsActive && !x.IsDeleted)
                   .Join(tenantDb.AssetTracking,
                       a => a.AssetId,
                       t => t.AssetId,
                       (a, t) => t)
                   .Where(x => !x.IsDeleted)
                   .Select(x => x.AssignedTo)
                   .FirstOrDefaultAsync();

        var notificationData = await _notificationBuilderService.BuildAsync(
                  _tenantFactory,
                  tenantId,
                  wo.UpdatedBy,
                  wo.AssignedToUserId,
                  wo.CreatedBy,
                  assignedToUserIdAsset
              );

        var eventDto = new WorkOrderEventDto
        {
            WorkOrderId = wo.WorkOrderId,
            TenantId = tenantId,
            WorkOrderType = wo.WorkOrderType,
            WorkOrderTypeName = wo.WorkOrderType?.ToString(),
            WorkOrderNumber = wo.WorkOrderNumber,
            
            Title = wo.Title,
            EventType = eventType,
            EventTime = DateTime.UtcNow,
            Priority = wo.Priority.ToString(),
            Status = wo.Status.ToString(),
            AssignedToUserId = wo.AssignedToUserId,
            AssignedToTeamId = wo.AssignedToTeamId,

            outgoingNotifications = null,
            incomingNotifications = notificationData.AllUsers,
            TargetUsersIds = notificationData.AllUsers,
            TargetEmailAddresses = notificationData.Emails
        };

        var topicName = KafkaCommonTopics.BuildTopicName(tenantId, eventType);

        // TRACE - BEFORE PUBLISH
        await _trace.TrackAsync(new EventTraceEntry
        {
            CorrelationId = correlationId,
            TenantId = tenantId,
            Service = "WorkOrderScheduler",
            Stage = "PUBLISH_REQUESTED",
            Topic = topicName,
            Status = "SUCCESS",
            Message = $"Scheduler event {eventType} for WO {wo.WorkOrderNumber}"
        });

        var kafkaRequest = new
        {
            Topic = topicName,
            Key = $"workorder-{wo.WorkOrderId}",
            Payload = eventDto,
            Source = "WorkOrderScheduler",
            Headers = new Dictionary<string, string>
            {
                ["tenant-id"] = tenantId.ToString(),
                ["correlation-id"] = correlationId
            }
        };

        try
        {
            var httpClient = _httpClientFactory.CreateClient();

            var jsonContent = new StringContent(
                JsonSerializer.Serialize(kafkaRequest),
                Encoding.UTF8,
                "application/json");

            var response = await httpClient.PostAsync(ConstantUrls.kafkaPublish, jsonContent);
            response.EnsureSuccessStatusCode();

            // TRACE SUCCESS
            await _trace.TrackAsync(new EventTraceEntry
            {
                CorrelationId = correlationId,
                TenantId = tenantId,
                Service = "WorkOrderScheduler",
                Stage = "PUBLISHED",
                Topic = topicName,
                Status = "SUCCESS"
            });

            _logger.LogInformation($"[Scheduler] Event Published: {eventType} for WO-{wo.WorkOrderId}");
        }
        catch (Exception ex)
        {
            // TRACE ERROR
            await _trace.TrackAsync(new EventTraceEntry
            {
                CorrelationId = correlationId,
                TenantId = tenantId,
                Service = "WorkOrderScheduler",
                Stage = "FAILED",
                Topic = topicName,
                Status = "ERROR",
                Error = ex.Message
            });

            _logger.LogError(ex, $"[Scheduler] Kafka publish failed for WO-{wo.WorkOrderId}");
            throw;
        }
    }
}