using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Repositories.TenantAdmin.Notification;
using FactoryOpsApp.Application.DTOs;
using FactoryOpsApp.Infrastructure.DBContext;
using Microsoft.EntityFrameworkCore;

namespace FactoryOperation_AccessManagementService.FactoryOpsApp.Infrastructure.Implementation.Repository.TenantAdmin.Notification
{
    public class NotificationQueryRepository : INotificationQueryRepository
    {
        private readonly TenantDbContextFactory _tenantDbContext;

        public NotificationQueryRepository(TenantDbContextFactory tenantDbContext)
        {
            _tenantDbContext = tenantDbContext;
        }

        public async Task<List<NotificationDto>> GetAllNotificationsAsync(int tenantId)
        {
            using var tenantDb = _tenantDbContext.GetTenantDbContext(tenantId);
            var notifications = await tenantDb.MasterNotifications
                .OrderByDescending(n => n.CreatedAt)
                .Select(n => new NotificationDto
                {
                    NotificationId = n.NotificationId,
                    TenantId = n.TenantId,
                    Module = n.Module,
                    EntityId = n.EntityId,
                    Title = n.Title,
                    Message = n.Message,
                    EventType = n.EventType,
                    NotificationType = n.NotificationType,
                    TargetUserId = n.TargetUserId,
                    TargetTeamId = n.TargetTeamId,
                    IsRead = n.IsRead,
                    CreatedAt = n.CreatedAt,
                    AdditionalData = n.AdditionalData.ToString()
                })
                .ToListAsync();

            return notifications;
        }
        public async Task<List<NotificationDto>> GetAllWONotification(int tenantId)
        {
            using var tenantDb = _tenantDbContext.GetTenantDbContext(tenantId);
            var notifications = await tenantDb.MasterNotifications.Where(x => x.Module.ToLower() == "workorder")
                .OrderByDescending(n => n.CreatedAt)
                .Select(n => new NotificationDto
                {
                    NotificationId = n.NotificationId,
                    TenantId = n.TenantId,
                    Module = n.Module,
                    EntityId = n.EntityId,
                    Title = n.Title,
                    Message = n.Message,
                    EventType = n.EventType,
                    NotificationType = n.NotificationType,
                    TargetUserId = n.TargetUserId,
                    TargetTeamId = n.TargetTeamId,
                    IsRead = n.IsRead,
                    CreatedAt = n.CreatedAt,
                    AdditionalData = n.AdditionalData.ToString()
                })
                .ToListAsync();

            return notifications;
        }
        /* public async Task<List<NotificationDto>> GetUnreadNotificationsAsync(int tenantId)
         {
             using var tenantDb = _tenantDbContext.GetTenantDbContext(tenantId);
             return await tenantDb.MasterNotifications
                 .Where(n => !n.IsRead)
                 .OrderByDescending(n => n.CreatedAt)
                 .Select(n => new NotificationDto
                 {
                     NotificationId = n.NotificationId,
                     TenantId = n.TenantId,
                     Module = n.Module,
                     Title = n.Title,
                     EventType = n.EventType,
                     Message = n.Message,
                     NotificationType = n.NotificationType,
                     CreatedAt = n.CreatedAt,
                     IsRead = n.IsRead
                 })
                 .ToListAsync();
         }*/

        public async Task<List<NotificationDto>> GetUnreadNotificationsAsync(int tenantId, int userId)
        {
            using var tenantDb = _tenantDbContext.GetTenantDbContext(tenantId);
            var notifications = await (
            from n in tenantDb.MasterNotifications
            join m in tenantDb.NotificationsMappings
                on n.NotificationId equals m.MasterNotificationId
            where m.TargetUsersId == userId && !m.IsRead
            orderby n.CreatedAt descending
            select new NotificationDto
            {
                NotificationId = n.NotificationId,
                TenantId = n.TenantId,
                Module = n.Module,
                Title = n.Title,
                EventType = n.EventType,
                Message = n.Message,
                NotificationType = n.NotificationType,
                CreatedAt = n.CreatedAt,
                IsRead = m.IsRead
            })
            .ToListAsync();
            return notifications;
        }

        public async Task<List<NotificationDto>> GetUserNotificationsAsync(int tenantId, int userId)
        {
            using var tenantDb = _tenantDbContext.GetTenantDbContext(tenantId);

            var notifications = await (
                from n in tenantDb.MasterNotifications
                join m in tenantDb.NotificationsMappings
                    on n.NotificationId equals m.MasterNotificationId
                where m.TargetUsersId == userId
                orderby n.CreatedAt descending
                select new NotificationDto
                {
                    NotificationId = n.NotificationId,
                    TenantId = n.TenantId,
                    Module = n.Module,
                    Title = n.Title,
                    EventType = n.EventType,
                    Message = n.Message,
                    NotificationType = n.NotificationType,
                    CreatedAt = n.CreatedAt,
                    IsRead = m.IsRead
                })
                .ToListAsync();

            return notifications;
        }


        /*        public async Task<List<NotificationDto>> GetUserNotificationsAsync(int tenantId, int userId)
                {
                    using var tenantDb = _tenantDbContext.GetTenantDbContext(tenantId);
                    return await tenantDb.MasterNotifications
                        .Where(n => n.TargetUserId == userId || n.CreatedByUserId == userId)
                        .OrderByDescending(n => n.CreatedAt)
                        .Select(n => new NotificationDto
                        {
                            NotificationId = n.NotificationId,
                            TenantId = n.TenantId,
                            Module = n.Module,
                            Title = n.Title,
                            EventType = n.EventType,
                            Message = n.Message,
                            NotificationType = n.NotificationType,
                            CreatedAt = n.CreatedAt,
                            IsRead = n.IsRead
                        })
                        .ToListAsync();

                }*/

        /* public async Task<bool> MarkAsReadAsync(int notificationId, int tenantId)
         {
             using var tenantDb = _tenantDbContext.GetTenantDbContext(tenantId);
             var notification = await tenantDb.MasterNotifications
                 .FirstOrDefaultAsync(n => n.NotificationId == notificationId);

             if (notification == null) return false;

             notification.IsRead = true;
             await tenantDb.SaveChangesAsync();
             return true;
         }*/
        public async Task<bool> MarkNotificationAsReadAsync(int notificationId, int userId, int tenantId)
        {

            using var tenantDb = _tenantDbContext.GetTenantDbContext(tenantId);
            var mappingEntry = await tenantDb.NotificationsMappings
                .FirstOrDefaultAsync(x =>
                    x.MasterNotificationId == notificationId &&
                    x.TargetUsersId == userId);

            mappingEntry.IsRead = true;
            mappingEntry.DeletedAt = null;
            await tenantDb.SaveChangesAsync();

            return true;
        }
    }
}
