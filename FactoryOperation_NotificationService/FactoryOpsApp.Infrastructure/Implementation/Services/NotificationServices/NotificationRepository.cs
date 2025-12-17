using FactoryOperation_NotificationService.FactoryOpsApp.Application.Interfaces.Kafka_Notification;
using FactoryOperation_NotificationService.FactoryOpsApp.Common.Models;
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

        public async Task SaveAsync(NotificationModel model)
        {
            await using var tenantDb = _tenantDbFactory.GetTenantDbContext(model.TenantId);

            var entity = new MasterNotification
            {
                TenantId = model.TenantId,
                Module = "WorkOrder",
                EntityId = model.WorkOrderId,
                Title = model.Title,
                Message = model.Message,
                NotificationType = model.EventType,
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
            allTargetUserIds.AddRange(model.TargetUserIds);
            //if (model.CreatedByUserId.HasValue)
            //    allTargetUserIds.Add(model.CreatedByUserId.Value);
            //if (nullableUserIds != null && nullableUserIds.Any())
            //    allTargetUserIds.AddRange(nullableUserIds);
            foreach (var userId in allTargetUserIds)
            {
                var notifMap = new NotificationsMapping
                {
                    MasterNotificationId = entity.NotificationId,
                    WorkOrderId = model.WorkOrderId,
                    TargetUsersId = userId,
                    CreatedBy = model.CreatedByUserId,
                    TenantId = model.TenantId,
                    CreatedAt = DateTime.UtcNow,
                    IsRead = false
                };

                tenantDb.NotificationsMappings.Add(notifMap);
            }

            await tenantDb.SaveChangesAsync();
        }

    }
}


