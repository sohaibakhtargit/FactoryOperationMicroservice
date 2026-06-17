using FactoryOperation_NotificationService.FactoryOpsApp.Application.Interfaces.Kafka_Notification;
using FactoryOperation_NotificationService.FactoryOpsApp.Common.Models;
using FactoryOpsApp.Domain.Entities;
using FactoryOpsApp.Domain.Entities.FactoryOpsTenants;
using FactoryOpsApp.Infrastructure.DBContext;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace FactoryOperation_NotificationService.FactoryOpsApp.Infrastructure.Implementation.Services.NotificationServices
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly TenantDbContextFactory _tenantDbFactory;

        public NotificationRepository(TenantDbContextFactory tenantDbFactory)
        {
            _tenantDbFactory = tenantDbFactory;
        }

        public async Task<int> SaveAsync(NotificationModel model)
        {
            await using var tenantDb = _tenantDbFactory.GetTenantDbContext(model.TenantId);

            var entity = new MasterNotification
            {
                TenantId = model.TenantId,
                Module = "WorkOrder",
                EntityId = model.WorkOrderId,
                ServiceRequestId = model.ServiceRequestId,
                WorkOrderNumber = model.WorkOrderNumber,
                ItemCode = model.ItemCode!,
                ServiceRequestNumber = model.ServiceRequestNumber,
                Title = model.Title,
                Message = model.Message,
                EventType = model.EventType,
                NotificationType = model.NotificationType,
                WorkOrderType = model.WorkOrderTypeName,
                TargetUserId = model.TargetUserId,
                TargetTeamId = model.TargetTeamId,
                CreatedByUserId = model.CreatedByUserId,
                CreatedAt = DateTime.UtcNow,
                UpdatedBy = model.UpdatedBy,
                UpdatedAt = model.UpdatedBy.HasValue ? DateTime.UtcNow : null,
                IsRead = false,
                AdditionalData = model.AdditionalDataJson != null
                    ? JsonDocument.Parse(model.AdditionalDataJson)
                    : null,
                OutgoingNotification = model.outgoingNotifications
            };

            tenantDb.MasterNotifications.Add(entity);
            await tenantDb.SaveChangesAsync();

            var allTargetUserIncomingIds = new List<int?>();

            if (model.incomingNotifications != null && model.incomingNotifications.Any())
            {
                allTargetUserIncomingIds.AddRange(model.incomingNotifications);
            }

            foreach (var userId in allTargetUserIncomingIds)
            {
                var notifMap = new NotificationsMapping
                {
                    MasterNotificationId = entity.NotificationId,
                    WorkOrderId = model.WorkOrderId,
                    ServiceRequestId = model.ServiceRequestId,
                    TargetUsersId = userId,
                   // CreatedBy = model.CreatedByUserId,
                   // UpdatedBy = model.outgoingNotifications,
                    TenantId = model.TenantId,
                    CreatedAt = DateTime.UtcNow,
                    IsRead = false,
                   
                    // your logic
                    /* IncomingNotifications = userId,
                     OutgoingNotifications = model.CreatedByUserId*/
                    IncomingNotifications = userId,

                };

                tenantDb.NotificationsMappings.Add(notifMap);
            }

            await tenantDb.SaveChangesAsync();

            return entity.NotificationId;
        }

      /*  public async Task SaveAsync(NotificationModel model)
        {
            await using var tenantDb = _tenantDbFactory.GetTenantDbContext(model.TenantId);

            var entity = new MasterNotification
            {
                TenantId = model.TenantId,
                Module = "WorkOrder",
                EntityId = model.WorkOrderId,
                ServiceRequestId = model.ServiceRequestId,
                WorkOrderNumber = model.WorkOrderNumber,
                ItemCode = model.ItemCode!,
                ServiceRequestNumber = model.ServiceRequestNumber,
                Title = model.Title,
                Message = model.Message,
                NotificationType = model.EventType,
                WorkOrderType = model.WorkOrderTypeName,
                TargetUserId = model.TargetUserId,
                TargetTeamId = model.TargetTeamId,
                CreatedByUserId = model.CreatedByUserId,
                CreatedAt = DateTime.UtcNow,
                IsRead = false,
                AdditionalData = model.AdditionalDataJson != null
                    ? JsonDocument.Parse(model.AdditionalDataJson)
                    : null
            };

            tenantDb.MasterNotifications.Add(entity);
            await tenantDb.SaveChangesAsync();

            var allTargetUserIds = new List<int?>();

            if (model.TargetUserIds != null && model.TargetUserIds.Any())
            {
                allTargetUserIds.AddRange(model.TargetUserIds);
            }

            foreach (var userId in allTargetUserIds)
            {
                var notifMap = new NotificationsMapping
                {
                    MasterNotificationId = entity.NotificationId,
                    WorkOrderId = model.WorkOrderId,
                    ServiceRequestId = model.ServiceRequestId,
                    TargetUsersId = userId,
                    CreatedBy = model.CreatedByUserId,
                    TenantId = model.TenantId,
                    CreatedAt = DateTime.UtcNow,
                    IsRead = false,
                    IncomingNotifications = userId,
                    OutgoingNotifications = model.CreatedByUserId
                };

                tenantDb.NotificationsMappings.Add(notifMap);
            }

            await tenantDb.SaveChangesAsync();

        }*/

    }
}


