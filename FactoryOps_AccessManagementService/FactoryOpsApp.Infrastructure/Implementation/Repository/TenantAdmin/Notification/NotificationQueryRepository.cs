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
                    ServiceRequestId = n.ServiceRequestId,
                    EntityId = n.EntityId,
                    Title = n.Title!,
                    Message = n.Message!,
                    EventType = n.EventType,
                    workOrderTypeName = n.WorkOrderType,
                    WorkOrderNumber = n.WorkOrderNumber,
                    ItemCode = n.ItemCode ?? "",
                    NotificationType = n.NotificationType,
                    TargetUserId = n.TargetUserId,
                    TargetTeamId = n.TargetTeamId,
                    IsRead = n.IsRead,
                    CreatedAt = n.CreatedAt,
                    CreatedBy = n.CreatedByUserId,
                    AdditionalData = n.AdditionalData!.ToString()
                })
                .ToListAsync();

            return notifications;
        }
        public async Task<List<NotificationDto>> GetAllWONotification(int tenantId)
        {
            using var tenantDb = _tenantDbContext.GetTenantDbContext(tenantId);
            var notifications = await tenantDb.MasterNotifications.Where(x => x.Module!.ToLower() == "workorder")
                .OrderByDescending(n => n.CreatedAt)
                .Select(n => new NotificationDto
                {
                    NotificationId = n.NotificationId,
                    TenantId = n.TenantId,
                    Module = n.Module,
                    ServiceRequestId = n.ServiceRequestId,
                    EntityId = n.EntityId,
                    Title = n.Title!,
                    Message = n.Message!,
                    EventType = n.EventType,
                    NotificationType = n.NotificationType,
                    workOrderTypeName = n.WorkOrderType,
                    WorkOrderNumber = n.WorkOrderNumber,
                    TargetUserId = n.TargetUserId,
                    TargetTeamId = n.TargetTeamId,
                    IsRead = n.IsRead,
                    CreatedAt = n.CreatedAt,
                    CreatedBy = n.CreatedByUserId,
                    AdditionalData = n.AdditionalData!.ToString(),
                    ItemCode = n.ItemCode ?? "",
                })
                .ToListAsync();

            return notifications;
        }
        public async Task<List<NotificationDto>> GetUserIncomingNotifications(int tenantId, int? userId)
        {
            using var tenantDb = _tenantDbContext.GetTenantDbContext(tenantId);

            var query = from n in tenantDb.MasterNotifications
                        join m in tenantDb.NotificationsMappings
                            on n.NotificationId equals m.MasterNotificationId
                        where m.TenantId == tenantId
                              && m.IncomingNotifications != null   
                        orderby n.CreatedAt descending
                        select new { n, m };

            
            if (userId.HasValue && userId.Value > 0)
            {
                query = query.Where(x => x.m.TargetUsersId == userId.Value);
            }
            else
            {
                query = query.Where(x => x.m.TargetUsersId == userId && x.m.TenantId == tenantId);
            }

            var notifications = await query.Select(x => new NotificationDto
            {
                NotificationId = x.n.NotificationId,
                TenantId = x.n.TenantId,
                Module = x.n.Module,
                EntityId = x.n.EntityId,

                Title = x.n.Title!,
                Message = x.n.Message!,
                EventType = x.n.EventType,
                NotificationType = x.n.NotificationType,

                workOrderId = x.m.WorkOrderId ?? 0,
                WorkOrderNumber = x.n.WorkOrderNumber,
                workOrderTypeName = x.n.WorkOrderType,

                ServiceRequestId = x.m.ServiceRequestId,
                ServiceRequestNumber = x.n.ServiceRequestNumber,

                TargetUserId = x.m.TargetUsersId,
                IncomingNotifications = x.m.IncomingNotifications,

                IsRead = x.m.IsRead,
                IsReadByTenant = x.m.IsReadByTenant,

                CreatedAt = x.n.CreatedAt,
                CreatedBy = x.n.CreatedByUserId,
                ItemCode = x.n.ItemCode ?? ""
            }).ToListAsync();

            return notifications;
        }
        //public async Task<List<NotificationDto>> GetUserOutgoingNotifications(int tenantId, int? userId)
        //{
        //    using var tenantDb = _tenantDbContext.GetTenantDbContext(tenantId);

        //    var query = from n in tenantDb.MasterNotifications
        //                join m in tenantDb.NotificationsMappings
        //                    on n.NotificationId equals m.MasterNotificationId
        //                where m.TenantId == tenantId
        //                      && n.OutgoingNotification != null   
        //                orderby n.CreatedAt descending
        //                select new { n, m };


        //    if (userId.HasValue && userId.Value > 0)
        //    {
        //        query = query.Where(x => x.m.TargetUsersId == userId);
        //    }
        //    else
        //    {
        //        query = query.Where(x => x.m.TargetUsersId == userId && x.m.TenantId == tenantId);
        //    }
        //    var notifications = await query.Select(x => new NotificationDto
        //    {
        //        NotificationId = x.n.NotificationId,
        //        TenantId = x.n.TenantId,
        //        Module = x.n.Module,
        //        EntityId = x.n.EntityId,

        //        Title = x.n.Title,
        //        Message = x.n.Message,
        //        EventType = x.n.EventType,
        //        NotificationType = x.n.NotificationType,

        //        workOrderId = x.m.WorkOrderId ?? 0,
        //        WorkOrderNumber = x.n.WorkOrderNumber,
        //        workOrderTypeName = x.n.WorkOrderType,

        //        ServiceRequestId = x.m.ServiceRequestId,
        //        ServiceRequestNumber = x.n.ServiceRequestNumber,

        //        TargetUserId = x.m.TargetUsersId,
        //        OutgoingNotifications = x.n.OutgoingNotification,

        //        IsRead = x.m.IsRead,
        //        IsReadByTenant = x.m.IsReadByTenant,

        //        CreatedAt = x.n.CreatedAt,

        //        ItemCode = x.n.ItemCode ?? "",

        //    }).ToListAsync();

        //    return notifications;
        //}

        public async Task<List<NotificationDto>> GetUserOutgoingNotifications(int tenantId, int? userId)
        {
            using var tenantDb = _tenantDbContext.GetTenantDbContext(tenantId);

            var outgoingNotifications = await (
                from n in tenantDb.MasterNotifications
                where  n.OutgoingNotification == userId
                orderby n.CreatedAt descending
                select new NotificationDto
                {
                    NotificationId = n.NotificationId,
                    TenantId = n.TenantId,
                    Module = n.Module,
                    EntityId = n.EntityId,
                    Title = n.Title!,
                    Message = n.Message!,
                    EventType = n.EventType,
                    NotificationType = n.NotificationType,
                    workOrderId = n.EntityId ?? 0,
                    WorkOrderNumber = n.WorkOrderNumber,
                    workOrderTypeName = n.WorkOrderType,
                    ServiceRequestId = n.ServiceRequestId,
                    ServiceRequestNumber = n.ServiceRequestNumber,
                    OutgoingNotifications = n.OutgoingNotification,
                    IsRead = n.IsRead,
                    IsReadByTenant = n.IsReadByTenant,
                    CreatedAt = n.CreatedAt,
                    CreatedBy = n.CreatedByUserId,
                    ItemCode = n.ItemCode ?? "",
                }).ToListAsync();

            return outgoingNotifications;
        }

        public async Task<List<NotificationDto>> GetUnreadNotificationsAsync(int tenantId, int? userId)
        {
            using var tenantDb = _tenantDbContext.GetTenantDbContext(tenantId);

            // =========================
            // INCOMING UNREAD
            // =========================
            var incomingQuery =
                from n in tenantDb.MasterNotifications
                join m in tenantDb.NotificationsMappings
                    on n.NotificationId equals m.MasterNotificationId
                where m.TenantId == tenantId
                select new NotificationDto
                {
                    NotificationId = n.NotificationId,
                    TenantId = n.TenantId,
                    Module = n.Module,
                    Title = n.Title!,
                    workOrderTypeName = n.WorkOrderType,

                    workOrderId = m.WorkOrderId ?? 0,
                    ServiceRequestId = m.ServiceRequestId,
                    ServiceRequestNumber = n.ServiceRequestNumber,

                    ItemCode = n.ItemCode ?? "",
                    EventType = n.EventType,
                    Message = n.Message!,
                    NotificationType = n.NotificationType,
                    WorkOrderNumber = n.WorkOrderNumber,

                    CreatedAt = n.CreatedAt,
                    CreatedBy = n.CreatedByUserId,
                    IsRead = m.IsRead,
                    IsReadByTenant = m.IsReadByTenant,

                    IncomingNotifications = m.IncomingNotifications,
                    OutgoingNotifications = m.OutgoingNotifications,

                    TargetUserId = m.TargetUsersId
                };

            if (userId.HasValue && userId.Value > 0)
            {
                incomingQuery = incomingQuery
                    .Where(x => x.TargetUserId == userId.Value && !x.IsRead);
            }
            else
            {
                incomingQuery = incomingQuery
                    .Where(x => !x.IsReadByTenant);
            }

            // =========================
            // OUTGOING UNREAD
            // =========================
            var outgoingQuery =
                from n in tenantDb.MasterNotifications
                where n.TenantId == tenantId
                      && n.OutgoingNotification != null
                select new NotificationDto
                {
                    NotificationId = n.NotificationId,
                    TenantId = n.TenantId,
                    Module = n.Module,
                    Title = n.Title!,
                    workOrderTypeName = n.WorkOrderType,

                    workOrderId = n.EntityId ?? 0,
                    ServiceRequestId = n.ServiceRequestId,
                    ServiceRequestNumber = n.ServiceRequestNumber,

                    ItemCode = n.ItemCode ?? "",
                    EventType = n.EventType,
                    Message = n.Message!,
                    NotificationType = n.NotificationType,
                    WorkOrderNumber = n.WorkOrderNumber,

                    CreatedAt = n.CreatedAt,

                    IsRead = n.IsRead,
                    IsReadByTenant = n.IsReadByTenant,
                    CreatedBy = n.CreatedByUserId,
                    //  IMPORTANT FIX (align DTO)
                    IncomingNotifications = null,
                    OutgoingNotifications = n.OutgoingNotification,
                    TargetUserId = null
                };

            if (userId.HasValue && userId.Value > 0)
            {
                outgoingQuery = outgoingQuery
                    .Where(x => x.OutgoingNotifications == userId.Value && !x.IsRead);
            }
            else
            {
                outgoingQuery = outgoingQuery
                    .Where(x => !x.IsReadByTenant);
            }

            // =========================
            // MERGE (SAFE)
            // =========================
            var incomingList = await incomingQuery.ToListAsync();
            var outgoingList = await outgoingQuery.ToListAsync();

            var result = incomingList
                .Concat(outgoingList)
                .OrderByDescending(x => x.CreatedAt)
                .GroupBy(x => x.NotificationId) // remove duplicates
                .Select(g => g.First())
                .ToList();

            return result;
        }
        /* public async Task<List<NotificationDto>> GetUnreadNotificationsAsync(int tenantId, int userId)
         {
             using var tenantDb = _tenantDbContext.GetTenantDbContext(tenantId);
             var notifications = await (
             from n in tenantDb.MasterNotifications
             join m in tenantDb.NotificationsMappings
                 on n.NotificationId equals m.MasterNotificationId
             where m.TargetUsersId == userId && m.IsRead == false
             orderby n.CreatedAt descending
             select new NotificationDto
             {
                 NotificationId = n.NotificationId,
                 TenantId = n.TenantId,
                 Module = n.Module,
                 Title = n.Title,
                 workOrderTypeName = n.WorkOrderType,
                 workOrderId = m.WorkOrderId ?? 0,
              //   workOrderId = m.WorkOrderId!.Value,
                 ServiceRequestId = m.ServiceRequestId,
                 ServiceRequestNumber = n.ServiceRequestNumber,
                 EventType = n.EventType,
                 Message = n.Message,
                 NotificationType = n.NotificationType,
                 WorkOrderNumber = n.WorkOrderNumber,
                 CreatedAt = n.CreatedAt,
                 IsRead = m.IsRead,
                 IncomingNotifications = m.IncomingNotifications,
                 OutgoingNotifications = m.OutgoingNotifications

             })
             .ToListAsync();
             return notifications;
         }
 */
        /* public async Task<List<NotificationDto>> GetUserNotificationsAsync(int tenantId, int? userId)
         {
             using var tenantDb = _tenantDbContext.GetTenantDbContext(tenantId);

             var query =
                 from n in tenantDb.MasterNotifications
                 join m in tenantDb.NotificationsMappings
                     on n.NotificationId equals m.MasterNotificationId
                 where m.TenantId == tenantId
                 select new { n, m };

             if (userId.HasValue)
             {
                 query = query.Where(x => x.m.TargetUsersId == userId.Value);
             }

             var notifications = await query
                 .OrderByDescending(x => x.n.CreatedAt)
                 .Select(x => new NotificationDto
                 {
                     NotificationId = x.n.NotificationId,
                     TenantId = x.n.TenantId,
                     Module = x.n.Module,
                     Title = x.n.Title,
                     EventType = x.n.EventType,
                     workOrderId = x.m.WorkOrderId ?? 0,
                     workOrderTypeName = x.n.WorkOrderType,
                     Message = x.n.Message,
                     NotificationType = x.n.NotificationType,
                     WorkOrderNumber = x.n.WorkOrderNumber,
                     CreatedAt = x.n.CreatedAt,

                     IsRead = userId.HasValue ? x.m.IsRead : x.m.IsReadByTenant,

                     IncomingNotifications = x.m.IncomingNotifications,
                     OutgoingNotifications = x.m.OutgoingNotifications
                 })
                 .ToListAsync();

             return notifications;
         }*/

        //public async Task<List<NotificationDto>> GetUserNotificationsAsync(int tenantId, int? userId)
        //{
        //    using var tenantDb = _tenantDbContext.GetTenantDbContext(tenantId);

        //    var baseQuery =
        //        from n in tenantDb.MasterNotifications
        //        join m in tenantDb.NotificationsMappings
        //            on n.NotificationId equals m.MasterNotificationId
        //        where m.TenantId == tenantId && n.TenantId == tenantId
        //        select new { n, m };

        //    if (userId.HasValue)
        //    {
        //        return await baseQuery
        //            .Where(x => x.m.TargetUsersId == userId.Value)
        //            .OrderByDescending(x => x.n.CreatedAt)
        //            .Select(x => new NotificationDto
        //            {
        //                NotificationId = x.n.NotificationId,
        //                TenantId = x.n.TenantId,
        //                Module = x.n.Module,
        //                Title = x.n.Title,
        //                EventType = x.n.EventType,
        //                workOrderId = x.m.WorkOrderId ?? 0,
        //                ServiceRequestId = x.m.ServiceRequestId,
        //                ServiceRequestNumber = x.n.ServiceRequestNumber,
        //                ItemCode = x.n.ItemCode ?? "",
        //                workOrderTypeName = x.n.WorkOrderType,
        //                Message = x.n.Message,
        //                NotificationType = x.n.NotificationType,
        //                WorkOrderNumber = x.n.WorkOrderNumber,
        //                CreatedAt = x.n.CreatedAt,
        //                IsRead = x.m.IsRead,
        //                IncomingNotifications = x.m.IncomingNotifications,
        //                OutgoingNotifications = x.m.OutgoingNotifications
        //            })
        //            .ToListAsync();
        //    }

        //    var notifications = await baseQuery
        //    .GroupBy(x => new
        //    {
        //        x.n.NotificationId,
        //        x.n.Title,
        //        x.n.Message,
        //        x.n.Module,
        //        x.n.EventType,
        //        x.n.NotificationType,
        //        x.n.WorkOrderNumber,
        //        x.n.ServiceRequestNumber,
        //        x.n.WorkOrderType,
        //        x.n.CreatedAt,
        //        x.n.TenantId,
        //        x.m.WorkOrderId,
        //        x.m.ServiceRequestId
        //    })
        //    .Select(g => new NotificationDto
        //    {
        //        NotificationId = g.Key.NotificationId,
        //        TenantId = g.Key.TenantId,
        //        Module = g.Key.Module,
        //        Title = g.Key.Title,
        //        EventType = g.Key.EventType,
        //        workOrderId = g.Key.WorkOrderId ?? 0,
        //        ServiceRequestId = g.Key.ServiceRequestId,
        //        ServiceRequestNumber = g.Key.ServiceRequestNumber,
        //        workOrderTypeName = g.Key.WorkOrderType,
        //        Message = g.Key.Message,
        //        NotificationType = g.Key.NotificationType,
        //        WorkOrderNumber = g.Key.WorkOrderNumber,
        //        CreatedAt = g.Key.CreatedAt,
        //        IsReadByTenant = g.All(x => x.m.IsReadByTenant),
        //        IncomingNotifications = g.Max(x => x.m.IncomingNotifications),
        //        OutgoingNotifications = g.Max(x => x.m.OutgoingNotifications)
        //    })
        //    .OrderByDescending(x => x.CreatedAt)
        //    .ToListAsync();

        //    return notifications;
        //}


        public async Task<List<NotificationDto>> GetUserNotificationsAsync(int tenantId, int? userId)
        {
            using var tenantDb = _tenantDbContext.GetTenantDbContext(tenantId);

            // =========================
            // INCOMING
            // =========================
            var incomingQuery =
                from n in tenantDb.MasterNotifications
                join m in tenantDb.NotificationsMappings
                    on n.NotificationId equals m.MasterNotificationId
                where m.TenantId == tenantId
                      && m.IncomingNotifications != null
                select new NotificationDto
                {
                    NotificationId = n.NotificationId,
                    TenantId = n.TenantId,
                    Module = n.Module,
                    EntityId = n.EntityId,

                    Title = n.Title!,
                    Message = n.Message!,
                    EventType = n.EventType,
                    NotificationType = n.NotificationType,

                    workOrderId = m.WorkOrderId ?? 0,
                    WorkOrderNumber = n.WorkOrderNumber,
                    workOrderTypeName = n.WorkOrderType,

                    ServiceRequestId = m.ServiceRequestId,
                    ServiceRequestNumber = n.ServiceRequestNumber,

                    TargetUserId = m.TargetUsersId,
                    IncomingNotifications = m.IncomingNotifications,
                    OutgoingNotifications = m.OutgoingNotifications, // IMPORTANT

                    IsRead = m.IsRead,
                    IsReadByTenant = m.IsReadByTenant,
                    CreatedBy = n.CreatedByUserId,
                    CreatedAt = n.CreatedAt,
                    ItemCode = n.ItemCode ?? ""
                };

            if (userId.HasValue && userId.Value > 0)
            {
                incomingQuery = incomingQuery
                    .Where(x => x.TargetUserId == userId.Value);
            }

            // =========================
            // OUTGOING
            // =========================
            var outgoingQuery =
                from n in tenantDb.MasterNotifications
                where n.TenantId == tenantId
                      && n.OutgoingNotification != null
                select new NotificationDto
                {
                    NotificationId = n.NotificationId,
                    TenantId = n.TenantId,
                    Module = n.Module,
                    EntityId = n.EntityId,

                    Title = n.Title!,
                    Message = n.Message!,
                    EventType = n.EventType,
                    NotificationType = n.NotificationType,

                    workOrderId = n.EntityId ?? 0,
                    WorkOrderNumber = n.WorkOrderNumber,
                    workOrderTypeName = n.WorkOrderType,

                    ServiceRequestId = n.ServiceRequestId,
                    ServiceRequestNumber = n.ServiceRequestNumber,

                    // 🔥 FIX: align fields
                    TargetUserId = null,
                    IncomingNotifications = null,
                    OutgoingNotifications = n.OutgoingNotification,

                    IsRead = n.IsRead,
                    IsReadByTenant = n.IsReadByTenant,
                    CreatedBy = n.CreatedByUserId,

                    CreatedAt = n.CreatedAt,
                    ItemCode = n.ItemCode ?? ""
                };

            if (userId.HasValue && userId.Value > 0)
            {
                outgoingQuery = outgoingQuery
                    .Where(x => x.OutgoingNotifications == userId.Value);
            }

            // =========================
            // MERGE (SAFE)
            // =========================

            // OPTION 1 (Recommended - avoids EF translation issues)
            var incomingList = await incomingQuery.ToListAsync();
            var outgoingList = await outgoingQuery.ToListAsync();

            var notifications = incomingList
                .Concat(outgoingList)
                .OrderByDescending(x => x.CreatedAt)
                .GroupBy(x => x.NotificationId) // prevent duplicates
                .Select(g => g.First())
                .ToList();

            return notifications;
        }
        //public async Task<bool> MarkNotificationAsReadAsync(int notificationId, int userId, int tenantId)
        //{
        //    using var tenantDb = _tenantDbContext.GetTenantDbContext(tenantId);

        //    var mappings = await tenantDb.NotificationsMappings
        //        .Where(x =>
        //            x.MasterNotificationId == notificationId &&
        //            x.TargetUsersId == userId &&
        //            x.TenantId == tenantId &&
        //            x.IsRead == false)   
        //        .ToListAsync();

        //    if (!mappings.Any())
        //    {
        //        return false;
        //    }

        //    foreach (var mapping in mappings)
        //    {
        //        mapping.IsRead = true;
        //        mapping.DeletedAt = null;
        //    }

        //    await tenantDb.SaveChangesAsync();

        //    return true;
        //}
        public async Task<bool> MarkNotificationAsReadAsync(
                int tenantId,
                int? userId,
                int notificationId,
                string type 
            )
        {
            using var tenantDb = _tenantDbContext.GetTenantDbContext(tenantId);

            bool updated = false;

            // =========================
            // INCOMING
            // =========================
            if (type == "incoming" || type == "bell")
            {
                if (userId.HasValue && userId.Value > 0)
                {
                    var entry = await tenantDb.NotificationsMappings
                        .FirstOrDefaultAsync(x =>
                            x.TenantId == tenantId &&
                            x.MasterNotificationId == notificationId &&
                            x.TargetUsersId == userId.Value &&
                            !x.IsRead);

                    if (entry != null)
                    {
                        entry.IsRead = true;
                        updated = true;
                    }
                }
                else
                {
                    var entries = await tenantDb.NotificationsMappings
                        .Where(x =>
                            x.TenantId == tenantId &&
                            x.MasterNotificationId == notificationId &&
                            !x.IsReadByTenant)
                        .ToListAsync();

                    foreach (var item in entries)
                    {
                        item.IsReadByTenant = true;
                        updated = true;
                    }
                }
            }

            // =========================
            // OUTGOING
            // =========================
            if (type == "outgoing" || type == "bell")
            {
                if (userId.HasValue && userId.Value > 0)
                {
                    var entry = await tenantDb.MasterNotifications
                        .FirstOrDefaultAsync(x =>
                            x.TenantId == tenantId &&
                            x.NotificationId == notificationId &&
                            x.OutgoingNotification == userId.Value &&
                            !x.IsRead);

                    if (entry != null)
                    {
                        entry.IsRead = true;
                        updated = true;
                    }
                }
                else
                {
                    var entry = await tenantDb.MasterNotifications
                        .FirstOrDefaultAsync(x =>
                            x.TenantId == tenantId &&
                            x.NotificationId == notificationId &&
                            !x.IsReadByTenant);

                    if (entry != null)
                    {
                        entry.IsReadByTenant = true;
                        updated = true;
                    }
                }
            }

            await tenantDb.SaveChangesAsync();
            return updated;
        }

        /* public async Task<bool> MarkAllNotificationsAsReadAsync(int workOrderId, int? userId, int tenantId)
         {
             using var tenantDb = _tenantDbContext.GetTenantDbContext(tenantId);

             var query = tenantDb.NotificationsMappings
                 .Where(x =>
                     x.TenantId == tenantId &&
                     x.WorkOrderId == workOrderId &&
                     !x.IsRead);

             if (userId.HasValue)
             {
                 query = query.Where(x => x.TargetUsersId == userId.Value);
             }

             var notifications = await query.ToListAsync();

             if (!notifications.Any())
                 return false;

             foreach (var notification in notifications)
             {
                 notification.IsRead = true;
             }

             await tenantDb.SaveChangesAsync();

             return true;
         }*/

        public async Task<bool> MarkAllNotificationsAsReadAsync(int? workOrderId, int? ServiceRequestId, int? userId, int tenantId, int notificationId)
        {
            using var tenantDb = _tenantDbContext.GetTenantDbContext(tenantId);
            var query = tenantDb.NotificationsMappings
                        .Where(x => x.TenantId == tenantId && x.DeletedAt == null);

            if (ServiceRequestId.HasValue && workOrderId.HasValue )
            {
                query = query.Where(x => x.WorkOrderId == workOrderId && x.ServiceRequestId == ServiceRequestId && x.MasterNotificationId ==  notificationId);
            }
            else if (ServiceRequestId.HasValue)
            {
                query = query.Where(x => x.ServiceRequestId == ServiceRequestId && x.MasterNotificationId == notificationId);
            }
            else if (workOrderId.HasValue)
            {
                query = query.Where(x => x.WorkOrderId == workOrderId && x.MasterNotificationId == notificationId);
            }

            if (userId.HasValue)
            {
                query = query.Where(x => x.TargetUsersId == userId.Value && !x.IsRead);

                var mappingEntries = await query.ToListAsync();

                if (!mappingEntries.Any())
                    return false;

                foreach (var entry in mappingEntries)
                {
                    entry.IsRead = true;
                }
            }
            else
            {
                query = query.Where(x => !x.IsReadByTenant);

                var mappingEntries = await query.ToListAsync();

                if (!mappingEntries.Any())
                    return false;

                foreach (var entry in mappingEntries)
                {
                    entry.IsReadByTenant = true;
                }
            }


            await tenantDb.SaveChangesAsync();
            return true;
        }

        /*        public async Task<bool> MarkAllNotificationsAsReadAsync(int workOrderId, int? userId, int tenantId)
                {
                    using var tenantDb = _tenantDbContext.GetTenantDbContext(tenantId);

                    var mappingEntries = await tenantDb.NotificationsMappings
                        .Where(x =>
                            x.TenantId == tenantId &&
                            x.WorkOrderId == workOrderId &&
                            x.TargetUsersId == userId &&
                            x.DeletedAt == null &&
                            !x.IsRead)
                        .ToListAsync();

                    if (!mappingEntries.Any())
                        return false;

                    foreach (var entry in mappingEntries)
                    {
                        entry.IsRead = true;
                    }

                    await tenantDb.SaveChangesAsync();
                    return true;
                }
        */
        public async Task<List<NotificationDto>> GetUserNotificationsAsync(int tenantId, int userId)
        {
            return await GetUserNotificationsAsync(tenantId, (int?)userId);
        }

        public Task<List<NotificationDto>> GetUserOutgoingNotifications(int tenantId, int userId)
        {
            throw new NotImplementedException();
        }

        public Task<List<NotificationDto>> GetUserIncomingNotifications(int tenantId, int userId)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> MarkAllNotificationsAsReadAsync(
                int tenantId,
                int? userId,
                string type
            )
        {
            using var tenantDb = _tenantDbContext.GetTenantDbContext(tenantId);

            bool updated = false;

            // =========================
            // INCOMING
            // =========================
            if (type == "incoming" || type == "bell")
            {
                if (userId.HasValue && userId.Value > 0)
                {
                    var entries = await tenantDb.NotificationsMappings
                        .Where(x =>
                            x.TenantId == tenantId &&
                            x.TargetUsersId == userId.Value &&
                            !x.IsRead)
                        .ToListAsync();

                    foreach (var item in entries)
                    {
                        item.IsRead = true;
                        updated = true;
                    }
                }
                else
                {
                    var entries = await tenantDb.NotificationsMappings
                        .Where(x =>
                            x.TenantId == tenantId &&
                            !x.IsReadByTenant)
                        .ToListAsync();

                    foreach (var item in entries)
                    {
                        item.IsReadByTenant = true;
                        updated = true;
                    }
                }
            }

            // =========================
            // OUTGOING
            // =========================
            if (type == "outgoing" || type == "bell")
            {
                if (userId.HasValue && userId.Value > 0)
                {
                    var entries = await tenantDb.MasterNotifications
                        .Where(x =>
                            x.TenantId == tenantId &&
                            x.OutgoingNotification == userId.Value &&
                            !x.IsRead)
                        .ToListAsync();

                    foreach (var item in entries)
                    {
                        item.IsRead = true;
                        updated = true;
                    }
                }
                else
                {
                    var entries = await tenantDb.MasterNotifications
                        .Where(x =>
                            x.TenantId == tenantId &&
                            !x.IsReadByTenant)
                        .ToListAsync();

                    foreach (var item in entries)
                    {
                        item.IsReadByTenant = true;
                        updated = true;
                    }
                }
            }

            await tenantDb.SaveChangesAsync();
            return updated;
        }
    }
}
