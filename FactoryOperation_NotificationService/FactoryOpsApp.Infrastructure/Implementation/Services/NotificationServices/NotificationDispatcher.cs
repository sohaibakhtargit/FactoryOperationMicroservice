using Confluent.Kafka;
using FactoryOperation_NotificationService.FactoryOpsApp.Application.Interfaces.NotificationServices;
using FactoryOperation_NotificationService.FactoryOpsApp.Common.Models;
using FactoryOps.NotificationService.Infrastructure.Hubs;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR;
using System.Reflection.Metadata;
using System.Text;

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

        //public async Task DispatchAsync(WorkOrderEventDto evt)
        //{
        //    var tasks = new List<Task>();

        //    string tenantGroup = $"tenant-{evt.TenantId}";
        //    string methodName = $"WorkOrder{evt.EventType}";

        //    // Track users already notified to avoid duplicate sends
        //    var notifiedUsers = new HashSet<string>();

        //    try
        //    {
        //        var excludedUsers = evt.TargetUsersIds?
        //         .Where(x => x.HasValue)
        //         .Select(x => x!.Value.ToString())
        //         .ToList();

        //        tasks.Add(
        //            _hub.Clients.GroupExcept(tenantGroup, excludedUsers ?? new List<string>())
        //                .SendAsync(methodName, evt)
        //        );
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Failed sending tenant group notification (Tenant={Tenant})", evt.TenantId);
        //    }

        //    if (evt.AssignedToUserId.HasValue)
        //    {
        //        try
        //        {
        //            string userId = evt.AssignedToUserId.Value.ToString();

        //            if (!notifiedUsers.Contains(userId))
        //            {
        //                tasks.Add(
        //                    _hub.Clients.User(userId)
        //                        .SendAsync("IncomingWorkOrderNotification", evt)
        //                );

        //                notifiedUsers.Add(userId);
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            _logger.LogError(ex, "Failed sending user notification (UserId={UserId})", evt.AssignedToUserId);
        //        }
        //    }

        //    if (evt.AssignedToTeamId.HasValue)
        //    {
        //        try
        //        {
        //            string teamGroup = $"team-{evt.AssignedToTeamId.Value}";

        //            tasks.Add(
        //                _hub.Clients.Group(teamGroup)
        //                    .SendAsync("IncomingWorkOrderNotification", evt)
        //            );
        //        }
        //        catch (Exception ex)
        //        {
        //            _logger.LogError(ex, "Failed sending team notification (TeamId={TeamId})", evt.AssignedToTeamId);
        //        }
        //    }

        //    // Supervisor notifications (avoid duplicates)
        //    if (evt.TargetUsersIds != null)
        //    {
        //        foreach (var userIdNullable in evt.TargetUsersIds.Where(u => u.HasValue))
        //        {
        //            int userId = userIdNullable!.Value;
        //            string userKey = userId.ToString();

        //            try
        //            {
        //                if (!notifiedUsers.Contains(userKey))
        //                {
        //                    tasks.Add(
        //                        _hub.Clients.User(userKey)
        //                            .SendAsync("WorkOrderNotification", evt)
        //                    );

        //                    notifiedUsers.Add(userKey);
        //                }
        //            }
        //            catch (Exception ex)
        //            {
        //                _logger.LogError(
        //                    ex,
        //                    "Failed sending supervisor notification (SupervisorUserId={UserId})",
        //                    userId
        //                );
        //            }
        //        }
        //    }

        //    try
        //    {
        //        await Task.WhenAll(tasks);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "One or more SignalR send operations failed.");
        //    }

        //    _logger.LogInformation(
        //        "SignalR notification dispatched successfully (WorkOrderId={WO})",
        //        evt.WorkOrderId
        //    );
        //}

        //new updatedDispatcher
        public async Task DispatchAsync(WorkOrderEventDto evt)
        {
            var tasks = new List<Task>();

            var message = BuildWorkOrderEventMessage(evt);

            var notification = new
            {
                NotificationId = evt.NotificationId,
                WorkOrderId = evt.WorkOrderId,
                WorkOrderNumber = evt.WorkOrderNumber,
                Message = message,
                Status = evt.Status,
                EventType = evt.EventType,
                NotificationType = evt.EventType,
                CreatedAt = evt.EventTime
            };

            var senderId = evt.outgoingNotifications;

            // Incoming users (distinct)
            var incomingUsers = evt.incomingNotifications?
                .Where(x => x.HasValue)
                .Select(x => x!.Value)
                .Distinct()
                .ToList() ?? new List<int>();

            // SAFETY: ensure sender is not in incoming
            if (senderId.HasValue)
            {
                incomingUsers = incomingUsers
                    .Where(x => x != senderId.Value)
                    .ToList();
            }

            // Bell users = incoming + sender
            var bellUsers = new HashSet<int>(incomingUsers);
            if (senderId.HasValue)
                bellUsers.Add(senderId.Value);

            // Track sent (avoid accidental duplicates across channels)
            var sentIncoming = new HashSet<int>();
            var sentOutgoing = new HashSet<int>();
            var sentBell = new HashSet<int>();

            // ==============================
            // INCOMING
            // ==============================
            foreach (var userId in incomingUsers)
            {
                if (sentIncoming.Add(userId))
                {
                    tasks.Add(
                        _hub.Clients.User(userId.ToString())
                            .SendAsync("IncomingNotification", notification)
                    );
                }
            }

            // ==============================
            // OUTGOING (sender only)
            // ==============================
            if (senderId.HasValue && sentOutgoing.Add(senderId.Value))
            {
                tasks.Add(
                    _hub.Clients.User(senderId.Value.ToString())
                        .SendAsync("OutgoingNotification", notification)
                );
            }

            // ==============================
            // BELL (incoming + sender)
            // ==============================
            foreach (var userId in bellUsers)
            {
                if (sentBell.Add(userId))
                {
                    tasks.Add(
                        _hub.Clients.User(userId.ToString())
                            .SendAsync("BellNotification", notification)
                    );
                }
            }

            // ==============================
            //  TENANT (broadcast)
            // ==============================
            if (evt.TenantId > 0)
            {
                string tenantGroup = $"tenant-{evt.TenantId}";

                tasks.Add(
                    _hub.Clients.Group(tenantGroup)
                        .SendAsync("TenantNotification", notification)
                );
            }

            await Task.WhenAll(tasks);

            _logger.LogInformation("Dispatch completed (WO={WO})", evt.WorkOrderId);
        }



        /*  public async Task DispatchAssignedToUserAsync(WorkOrderAssignedEventDto evt)
          {
              var tasks = new List<Task>();

              try
              {
                  if (evt.SupervisorUserIds != null)
                  {
                      foreach (var userId in evt.SupervisorUserIds)
                      {
                          if (userId.HasValue)
                          {
                              tasks.Add(
                                  _hub.Clients.User(userId.Value.ToString())
                                  .SendAsync("WorkOrderAssignedNotification", evt)
                              );
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
          }*/



        public async Task DispatchAssignedToUserAsync(WorkOrderAssignedEventDto evt)
        {
            var tasks = new List<Task>();

            //var message = BuildWorkOrderEventMessage(evt);

            var notification = new
            {
                NotificationId = evt.NotificationId,
                WorkOrderId = evt.WorkOrderId,
                WorkOrderNumber = evt.WorkOrderNumber,
                Message = $"AssignedTo {evt.AssignedTo} on workordernumber {evt.WorkOrderNumber}",
                Status = evt.Status,
                EventType = evt.EventType,
                NotificationType = evt.EventType,
                CreatedAt = evt.EventTime
            };

            var senderId = evt.outgoingNotifications;

            var incomingUsers = evt.incomingNotifications?
                .Where(x => x.HasValue)
                .Select(x => x!.Value)
                .Distinct()
                .ToList() ?? new List<int>();

            var bellUsers = new HashSet<int>(incomingUsers);

            if (senderId.HasValue)
                bellUsers.Add(senderId.Value);

            // Incoming
            foreach (var userId in incomingUsers)
            {
                tasks.Add(_hub.Clients.User(userId.ToString())
                    .SendAsync("IncomingNotification", notification));
            }

            // Outgoing
            if (senderId.HasValue)
            {
                tasks.Add(_hub.Clients.User(senderId.Value.ToString())
                    .SendAsync("OutgoingNotification", notification));
            }

            // Bell
            foreach (var userId in bellUsers)
            {
                tasks.Add(_hub.Clients.User(userId.ToString())
                    .SendAsync("BellNotification", notification));
            }

            // Tenant
            if (evt.TenantId > 0)
            {
                string tenantGroup = $"tenant-{evt.TenantId}";

                tasks.Add(_hub.Clients.Group(tenantGroup)
                    .SendAsync("TenantNotification", notification));
            }

            await Task.WhenAll(tasks);

            _logger.LogInformation("Dispatch completed (WO={WO})", evt.WorkOrderId);
        }
        public async Task DispatchWorkOrderDeletedAsync(WorkOrderEventDto evt)
        {
            var tasks = new List<Task>();

            try
            {
                var message = BuildWorkOrderEventMessage(evt);

                var notification = new
                {
                    NotificationId = evt.NotificationId,
                    WorkOrderId = evt.WorkOrderId,
                    WorkOrderNumber = evt.WorkOrderNumber,
                    Message = message,
                    Status = evt.Status,
                    EventType = evt.EventType,
                    NotificationType = evt.EventType,
                    CreatedAt = evt.EventTime
                };

                var senderId = evt.outgoingNotifications;

                // Incoming users
                var incomingUsers = evt.incomingNotifications?
                    .Where(x => x.HasValue)
                    .Select(x => x!.Value)
                    .Distinct()
                    .ToList() ?? new List<int>();

                // SAFETY: remove sender from incoming
                if (senderId.HasValue)
                {
                    incomingUsers = incomingUsers
                        .Where(x => x != senderId.Value)
                        .ToList();
                }

                // Bell users = incoming + sender
                var bellUsers = new HashSet<int>(incomingUsers);
                if (senderId.HasValue)
                    bellUsers.Add(senderId.Value);

                var sentIncoming = new HashSet<int>();
                var sentOutgoing = new HashSet<int>();
                var sentBell = new HashSet<int>();

                // ==============================
                // INCOMING
                // ==============================
                foreach (var userId in incomingUsers)
                {
                    if (sentIncoming.Add(userId))
                    {
                        tasks.Add(
                            _hub.Clients.User(userId.ToString())
                                .SendAsync("IncomingNotification", notification)
                        );
                    }
                }

                // ==============================
                // OUTGOING
                // ==============================
                if (senderId.HasValue && sentOutgoing.Add(senderId.Value))
                {
                    tasks.Add(
                        _hub.Clients.User(senderId.Value.ToString())
                            .SendAsync("OutgoingNotification", notification)
                    );
                }

                // ==============================
                // BELL
                // ==============================
                foreach (var userId in bellUsers)
                {
                    if (sentBell.Add(userId))
                    {
                        tasks.Add(
                            _hub.Clients.User(userId.ToString())
                                .SendAsync("BellNotification", notification)
                        );
                    }
                }

                // ==============================
                // TENANT
                // ==============================
                if (evt.TenantId > 0)
                {
                    string tenantGroup = $"tenant-{evt.TenantId}";

                    tasks.Add(
                        _hub.Clients.Group(tenantGroup)
                            .SendAsync("TenantNotification", notification)
                    );
                }

                await Task.WhenAll(tasks);

                _logger.LogInformation("Work order delete dispatch completed (WO={WO})", evt.WorkOrderId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to dispatch delete notification (WO={WO})", evt.WorkOrderId);
            }
        }


        /*   public async Task DispatchServiceRequestAsync(ServiceRequestEventDto evt)
           {
               var tasks = new List<Task>();

               if (evt.TargetUserIds != null)
               {
                   foreach (var userId in evt.TargetUserIds)
                   {
                       if (userId.HasValue)
                       {
                           tasks.Add(
                               _hub.Clients.User(userId.Value.ToString())
                                   .SendAsync("ServiceRequestCreated", evt)
                           );
                       }
                   }
               }

               await Task.WhenAll(tasks);

            _logger.LogInformation(
                "ServiceRequest notification dispatched (SR={SR})",
                evt.ServiceRequestId
            );
        }
        public async Task DispatchServiceRequestApprovedAsync(ServiceRequestEventDto evt)
        {
            if (evt.CreatedBy > 0)
            {
                await _hub.Clients.User(evt.CreatedBy.ToString()!)
                    .SendAsync("ServiceRequestApproved", evt);
            }
        }
        public async Task DispatchServiceRequestRejectedAsync(ServiceRequestEventDto evt)
        {
            if (evt.CreatedBy > 0)
            {
                await _hub.Clients.User(evt.CreatedBy.ToString()!)
                    .SendAsync("ServiceRequestRejected", evt);
            }
        }
        public async Task DispatchServiceRequestWorkOrderAssignedAsync(ServiceRequestEventDto evt)
        {
            var tasks = new List<Task>();
               _logger.LogInformation(
                   "ServiceRequest notification dispatched (SR={SR})",
                   evt.ServiceRequestId
               );
           }*/
        public async Task DispatchServiceRequestAsync(ServiceRequestEventDto evt)
        {
            var tasks = new List<Task>();
            var notificationType = evt.EventType;
            var eventType = "Created";
            evt.EventType = eventType;
            var message = BuildServiceRequestMessage(evt);

            var notification = new
            {
                NotificationId = evt.NotificationId,
                ServiceRequestId = evt.ServiceRequestId,
                ServiceRequestNumber = evt.ServiceRequestNumber,
                Message = message,
                Status = evt.Status,
                NotificationType = notificationType,
                EventType = eventType,
                CreatedBy = evt.CreatedBy,
                CreatedAt = evt.EventTime
            };

            var senderId = evt.outgoingNotifications;

            var incomingUsers = evt.incomingNotifications?
                .Where(x => x.HasValue)
                .Select(x => x!.Value)
                .Distinct()
                .ToList() ?? new List<int>();

            var bellUsers = new HashSet<int>(incomingUsers);

            if (senderId.HasValue)
                bellUsers.Add(senderId.Value);

            // ==============================
            // INCOMING
            // ==============================
            foreach (var userId in incomingUsers)
            {
                tasks.Add(
                    _hub.Clients.User(userId.ToString())
                        .SendAsync("IncomingNotification", notification)
                );
            }

            // ==============================
            // OUTGOING
            // ==============================
            if (senderId.HasValue)
            {
                tasks.Add(
                    _hub.Clients.User(senderId.Value.ToString())
                        .SendAsync("OutgoingNotification", notification)
                );
            }

            // ==============================
            // BELL (NO DUPLICATE)
            // ==============================
            foreach (var userId in bellUsers)
            {
                tasks.Add(
                    _hub.Clients.User(userId.ToString())
                        .SendAsync("BellNotification", notification)
                );
            }

            // ==============================
            // TENANT
            // ==============================
            if (evt.TenantId > 0)
            {
                string tenantGroup = $"tenant-{evt.TenantId}";

                tasks.Add(
                    _hub.Clients.Group(tenantGroup)
                        .SendAsync("TenantNotification", notification)
                );
            }

            await Task.WhenAll(tasks);

            _logger.LogInformation("Dispatch completed (SR={SR})", evt.ServiceRequestId);
        }
/*        public async Task DispatchServiceRequestAsync(ServiceRequestEventDto evt)
        {
            var tasks = new List<Task>();

            var message = BuildServiceRequestMessage(evt);

            var notification = new
            {
                NotificationId = evt.NotificationId,
                ServiceRequestId = evt.ServiceRequestId,
                ServiceRequestNumber = evt.ServiceRequestNumber,
                Message = message,
                Status = evt.Status,
                EventType = evt.EventType,
                CreatedAt = evt.EventTime
            };

            var senderId = evt.outgoingNotifications;
            if (evt.TargetUserIds != null)
            {
                foreach (var userId in evt.TargetUserIds)
                {
                    if (userId.HasValue)
                    {
                        tasks.Add(
                            _hub.Clients.User(userId.Value.ToString())
                                .SendAsync("BellNotification", evt)
                        );
                    }
                }
            }

            if (evt.incomingNotifications != null)
            {
                foreach (var userId in evt.incomingNotifications)
                {
                    if (userId.HasValue)
                    {
                        tasks.Add(
                            _hub.Clients.User(userId.Value.ToString())
                                .SendAsync("IncomingNotification", evt)
                        );
                    }
                }
            }

           
            if (evt.outgoingNotifications.HasValue)
            {
                tasks.Add(
                    _hub.Clients.User(evt.outgoingNotifications.Value.ToString())
                        .SendAsync("OutgoingNotification", evt)
                );
            }
            if (evt.TenantId > 0)
            {
                string tenantGroup = $"tenant-{evt.TenantId}";

                tasks.Add(
                    _hub.Clients.Group(tenantGroup)
                        .SendAsync("TenantNotification", evt)
                );
            }

            await Task.WhenAll(tasks);

            _logger.LogInformation("ServiceRequest notification dispatched (SR={SR})", evt.ServiceRequestId);
        }
*/        

        public async Task DispatchServiceRequestApprovedAsync(ServiceRequestEventDto evt)
        {
            var tasks = new List<Task>();
            var notificationType = evt.EventType;
            var eventType = "Approved";
            evt.EventType = eventType;
            var message = BuildServiceRequestMessage(evt);

            var notification = new
            {
                NotificationId = evt.NotificationId,
                ServiceRequestId = evt.ServiceRequestId,
                ServiceRequestNumber = evt.ServiceRequestNumber,
                WorkOrderId = evt.WorkOrderId,
                WorkOrderNumber = evt.WorkOrderNumber,
                Message = message,
                Status = evt.Status,
                EventType = eventType,
                NotificationType = notificationType,
                CreatedBy = evt.CreatedBy,
                CreatedAt = evt.EventTime
            };

            var senderId = evt.outgoingNotifications;

            var incomingUsers = evt.incomingNotifications?
                .Where(x => x.HasValue)
                .Select(x => x!.Value)
                .Distinct()
                .ToList() ?? new List<int>();

            var bellUsers = new HashSet<int>(incomingUsers);
            if (senderId.HasValue)
                bellUsers.Add(senderId.Value);

            foreach (var userId in incomingUsers)
            {
                tasks.Add(
                    _hub.Clients.User(userId.ToString())
                        .SendAsync("IncomingNotification", notification)
                );
            }

            if (senderId.HasValue)
            {
                tasks.Add(
                    _hub.Clients.User(senderId.Value.ToString())
                        .SendAsync("OutgoingNotification", notification)
                );
            }


            foreach (var userId in bellUsers)
            {
                tasks.Add(
                    _hub.Clients.User(userId.ToString())
                        .SendAsync("BellNotification", notification)
                );
            }


            if (evt.TenantId > 0)
            {
                string tenantGroup = $"tenant-{evt.TenantId}";

                tasks.Add(
                    _hub.Clients.Group(tenantGroup)
                        .SendAsync("TenantNotification", notification)
                );
            }

            await Task.WhenAll(tasks);

            _logger.LogInformation("Dispatch completed (SR Approved={SR})", evt.ServiceRequestId);
        }

      
        public async Task DispatchServiceRequestRejectedAsync(ServiceRequestEventDto evt)
        {
            var tasks = new List<Task>();
            var notificationType = evt.EventType;
            var eventType = "Rejected";
            evt.EventType = eventType;
            var message = BuildServiceRequestMessage(evt);

            var notification = new
            {
                NotificationId = evt.NotificationId,
                ServiceRequestId = evt.ServiceRequestId,
                ServiceRequestNumber = evt.ServiceRequestNumber,
                Message = message,
                Status = evt.Status,
                EventType = eventType,
                NotificationType = notificationType,
                CreatedAt = evt.EventTime,
                CreatedBy = evt.CreatedBy
            };

            var senderId = evt.outgoingNotifications;

            var incomingUsers = evt.incomingNotifications?
                .Where(x => x.HasValue)
                .Select(x => x!.Value)
                .Distinct()
                .ToList() ?? new List<int>();

            var bellUsers = new HashSet<int>(incomingUsers);
            if (senderId.HasValue)
                bellUsers.Add(senderId.Value);


            foreach (var userId in incomingUsers)
            {
                tasks.Add(
                    _hub.Clients.User(userId.ToString())
                        .SendAsync("IncomingNotification", notification)
                );
            }


            if (senderId.HasValue)
            {
                tasks.Add(
                    _hub.Clients.User(senderId.Value.ToString())
                        .SendAsync("OutgoingNotification", notification)
                );
            }

            foreach (var userId in bellUsers)
            {
                tasks.Add(
                    _hub.Clients.User(userId.ToString())
                        .SendAsync("BellNotification", notification)
                );
            }

            if (evt.TenantId > 0)
            {
                string tenantGroup = $"tenant-{evt.TenantId}";

                tasks.Add(
                    _hub.Clients.Group(tenantGroup)
                        .SendAsync("TenantNotification", notification)
                );
            }

            await Task.WhenAll(tasks);

            _logger.LogInformation("Dispatch completed (SR={SR})", evt.ServiceRequestId);
        }

        public async Task DispatchServiceRequestWorkOrderAssignedAsync(ServiceRequestEventDto evt)
        {
            var tasks = new List<Task>();
            var notificationType = evt.EventType;
            var eventType = "Assigned";
            evt.EventType = eventType;
            var message = BuildServiceRequestMessage(evt);

            var notification = new
            {
                NotificationId = evt.NotificationId,
                ServiceRequestId = evt.ServiceRequestId,
                ServiceRequestNumber = evt.ServiceRequestNumber,
                WorkOrderId = evt.WorkOrderId,
                WorkOrderNumber = evt.WorkOrderNumber,
                Message = message,
                Status = evt.Status,    
                EventType = eventType,
                NotificationType = notificationType,
                CreatedAt = evt.EventTime
            };

            var senderId = evt.outgoingNotifications;

            var incomingUsers = evt.incomingNotifications?
                .Where(x => x.HasValue)
                .Select(x => x!.Value)
                .Distinct()
                .ToList() ?? new List<int>();

            var bellUsers = new HashSet<int>(incomingUsers);

            if (senderId.HasValue)
                bellUsers.Add(senderId.Value);


            foreach (var userId in incomingUsers)
            {
                tasks.Add(
                    _hub.Clients.User(userId.ToString())
                        .SendAsync("IncomingNotification", notification)
                );
            }


            if (senderId.HasValue)
            {
                tasks.Add(
                    _hub.Clients.User(senderId.Value.ToString())
                        .SendAsync("OutgoingNotification", notification)
                );
            }


            foreach (var userId in bellUsers)
            {
                tasks.Add(
                    _hub.Clients.User(userId.ToString())
                        .SendAsync("BellNotification", notification)
                );
            }


            if (evt.TenantId > 0)
            {
                string tenantGroup = $"tenant-{evt.TenantId}";

                tasks.Add(
                    _hub.Clients.Group(tenantGroup)
                        .SendAsync("TenantNotification", notification)
                );
            }

            await Task.WhenAll(tasks);

            _logger.LogInformation("Dispatch completed (SR={SR})", evt.ServiceRequestId);
        }
        public async Task DispatchServiceRequestReopenedAsync(ServiceRequestEventDto evt)
        {
            var tasks = new List<Task>();
            var notificationType = evt.EventType;
            var eventType = "Reopened";
            evt.EventType = eventType;
            var message = BuildServiceRequestMessage(evt);

            var notification = new
            {
                NotificationId = evt.NotificationId,
                ServiceRequestId = evt.ServiceRequestId,
                ServiceRequestNumber = evt.ServiceRequestNumber,
                WorkOrderId = evt.WorkOrderId,
                WorkOrderNumber = evt.WorkOrderNumber,
                Message = message,
                Status = evt.Status,
                EventType = eventType,
                NotificationType = notificationType,
                CreatedAt = evt.EventTime
            };

            var senderId = evt.outgoingNotifications;

            var incomingUsers = evt.incomingNotifications?
                .Where(x => x.HasValue)
                .Select(x => x!.Value)
                .Distinct()
                .ToList() ?? new List<int>();

            var bellUsers = new HashSet<int>(incomingUsers);

            if (senderId.HasValue)
                bellUsers.Add(senderId.Value);

            foreach (var userId in incomingUsers)
            {
                tasks.Add(
                    _hub.Clients.User(userId.ToString())
                        .SendAsync("IncomingNotification", notification)
                );
            }


            if (senderId.HasValue)
            {
                tasks.Add(
                    _hub.Clients.User(senderId.Value.ToString())
                        .SendAsync("OutgoingNotification", notification)
                );
            }

            foreach (var userId in bellUsers)
            {
                tasks.Add(
                    _hub.Clients.User(userId.ToString())
                        .SendAsync("BellNotification", notification)
                );
            }

            if (evt.TenantId > 0)
            {
                string tenantGroup = $"tenant-{evt.TenantId}";

                tasks.Add(
                    _hub.Clients.Group(tenantGroup)
                        .SendAsync("TenantNotification", notification)
                );
            }

            await Task.WhenAll(tasks);

            _logger.LogInformation("Dispatch completed (SR={SR})", evt.ServiceRequestId);
        }

        //public async Task DispatchWorkOrderApproveRejectAsync(WorkOrderEventDto evt)
        //{

        //    var tasks = new List<Task>();

        //    if (evt.TargetUsersIds != null)
        //    {
        //        foreach (var userId in evt.TargetUsersIds)
        //        {
        //            if (userId.HasValue)
        //            {
        //                tasks.Add(
        //                    _hub.Clients.User(userId.Value.ToString())
        //                        .SendAsync("WorkOrderApproveRejectNotification", evt)
        //                );
        //            }
        //        }
        //    }

        //    await Task.WhenAll(tasks);
        //}

        public async Task DispatchWorkOrderApproveRejectAsync(WorkOrderEventDto evt)
        {
            var tasks = new List<Task>();
            var eventType = evt.EventType switch
            {
                "WorkOrderApproved" => "Approved",
                "WorkOrderRejected" => "Rejected",
                _ => "Updated"
            };

            try
            {
                var message = BuildWorkOrderEventMessage(evt);

                var notification = new
                {
                    NotificationId = evt.NotificationId,
                    WorkOrderId = evt.WorkOrderId,
                    WorkOrderNumber = evt.WorkOrderNumber,
                    Message = message,
                    Status = evt.Status,
                    EventType = eventType,
                    NotificationType = evt.EventType,
                    CreatedAt = evt.EventTime
                };

                var senderId = evt.outgoingNotifications;

                // Incoming users
                var incomingUsers = evt.incomingNotifications?
                    .Where(x => x.HasValue)
                    .Select(x => x!.Value)
                    .Distinct()
                    .ToList() ?? new List<int>();

                // SAFETY: remove sender from incoming
                if (senderId.HasValue)
                {
                    incomingUsers = incomingUsers
                        .Where(x => x != senderId.Value)
                        .ToList();
                }

                // Bell users
                var bellUsers = new HashSet<int>(incomingUsers);
                if (senderId.HasValue)
                    bellUsers.Add(senderId.Value);

                var sentIncoming = new HashSet<int>();
                var sentOutgoing = new HashSet<int>();
                var sentBell = new HashSet<int>();

                // ==============================
                // INCOMING
                // ==============================
                foreach (var userId in incomingUsers)
                {
                    if (sentIncoming.Add(userId))
                    {
                        tasks.Add(
                            _hub.Clients.User(userId.ToString())
                                .SendAsync("IncomingNotification", notification)
                        );
                    }
                }

                // ==============================
                // OUTGOING
                // ==============================
                if (senderId.HasValue && sentOutgoing.Add(senderId.Value))
                {
                    tasks.Add(
                        _hub.Clients.User(senderId.Value.ToString())
                            .SendAsync("OutgoingNotification", notification)
                    );
                }

                // ==============================
                // BELL
                // ==============================
                foreach (var userId in bellUsers)
                {
                    if (sentBell.Add(userId))
                    {
                        tasks.Add(
                            _hub.Clients.User(userId.ToString())
                                .SendAsync("BellNotification", notification)
                        );
                    }
                }

                // ==============================
                // TENANT
                // ==============================
                if (evt.TenantId > 0)
                {
                    string tenantGroup = $"tenant-{evt.TenantId}";

                    tasks.Add(
                        _hub.Clients.Group(tenantGroup)
                            .SendAsync("TenantNotification", notification)
                    );
                }

                await Task.WhenAll(tasks);

                _logger.LogInformation("Approve/Reject dispatch completed (WO={WO})", evt.WorkOrderId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to dispatch Approve/Reject notification (WO={WO})", evt.WorkOrderId);
            }
        }
        //public async Task DispatchLowStockAsync(LowStockEventDto evt)
        //{
        //    if (evt?.TargetUsersIds == null || !evt.TargetUsersIds.Any())
        //        return;

        //    var message = BuildLowStockMessage(evt);

        //    var notification = new
        //    {
        //        ItemId = evt.ItemId,
        //        ItemName = evt.ItemName,
        //        ItemCode = evt.ItemCode,
        //        Message = message,
        //        Type = evt.EventType,
        //        QuantityAvailable = evt.QuantityAvailable,
        //        EventTime = evt.EventTime
        //    };

        //    var tasks = evt.TargetUsersIds
        //        .Where(u => u.HasValue)
        //        .Select(u => u!.Value.ToString())
        //        .Distinct()
        //        .Select(userId =>
        //            _hub.Clients.User(userId)
        //                .SendAsync("LowStockNotification", notification)
        //        )
        //        .ToList();

        //    try
        //    {
        //        await Task.WhenAll(tasks);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Failed to dispatch LowStockNotification for Tenant {TenantId}", evt.TenantId);
        //    }
        //}

        public async Task DispatchLowStockAsync(LowStockEventDto evt)
        {
            if (evt == null) return;

            var eventType = evt.EventType switch
            {
                "LowStockWarning" => "Warning",
                "LowStockError" => "Error",
                _ => "Updated"
            };

            var tasks = new List<Task>();

            var message = BuildLowStockMessage(evt, eventType);

            var notification = new
            {
                NotificationId = evt.NotificationId,
                ItemId = evt.ItemId,
                ItemName = evt.ItemName,
                ItemCode = evt.ItemCode,

                Message = message,
                EventType = eventType,
                NotificationType = evt.EventType,
                QuantityAvailable = evt.QuantityAvailable,
                EventTime = evt.EventTime
            };

            // =========================
            // INCOMING USERS
            // =========================
            var incomingUsers = evt.incomingNotifications?
                .Where(x => x.HasValue)
                .Select(x => x!.Value)
                .Distinct()
                .ToList() ?? new List<int>();

            foreach (var userId in incomingUsers)
            {
                tasks.Add(
                    _hub.Clients.User(userId.ToString())
                        .SendAsync("IncomingNotification", notification)
                );
            }

            // =========================
            // TARGET USERS (ALERT)
            // =========================
            var targetUsers = evt.TargetUsersIds?
                .Where(x => x.HasValue)
                .Select(x => x!.Value)
                .Distinct()
                .ToList() ?? new List<int>();

            foreach (var userId in targetUsers)
            {
                tasks.Add(
                    _hub.Clients.User(userId.ToString())
                        .SendAsync("LowStockNotification", notification)
                );
            }

            // =========================
            // TENANT
            // =========================
            if (evt.TenantId > 0)
            {
                string tenantGroup = $"tenant-{evt.TenantId}";

                tasks.Add(
                    _hub.Clients.Group(tenantGroup)
                        .SendAsync("TenantNotification", notification)
                );
            }

            try
            {
                await Task.WhenAll(tasks);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to dispatch LowStockNotification for Tenant {TenantId}", evt.TenantId);
            }
        }

        //---End

        public async Task DispatchProgressUpdateAsync(WorkOrderProgressUpdatedEventDto evt)

        {
            var eventType = evt.Status switch
            {
                "InProgress" => "Started",
                "OnHold" => "Paused",
                "Completed" => "Completed",
                _ => "Updated"
            };

            var tasks = new List<Task>();

            var message = BuildWorkOrderMessage(evt);

            var notification = new

            {
                NotificationId = evt.NotificationId,
                WorkOrderId = evt.WorkOrderId,

                WorkOrderNumber = evt.WorkOrderNumber,

                Message = message,

                Status = evt.Status,
                EventType = eventType,
                NotificationType = evt.EventType,
                UpdatedAt = evt.UpdatedAt,

            };

            var senderId = evt.outgoingNotifications;

            var incomingUsers = evt.incomingNotifications?

                .Where(x => x.HasValue)

                .Select(x => x!.Value)

                .Distinct()

                .ToList() ?? new List<int>();

            if (senderId.HasValue)

            {

                incomingUsers = incomingUsers

                    .Where(x => x != senderId.Value)

                    .ToList();

            }


            var bellUsers = new HashSet<int>(incomingUsers);

            if (senderId.HasValue)

                bellUsers.Add(senderId.Value);

            var sentIncoming = new HashSet<int>();

            var sentOutgoing = new HashSet<int>();

            var sentBell = new HashSet<int>();

            foreach (var userId in incomingUsers)

            {

                if (sentIncoming.Add(userId))

                {

                    tasks.Add(

                        _hub.Clients.User(userId.ToString())

                            .SendAsync("IncomingNotification", notification)

                    );

                }

            }

            if (senderId.HasValue && sentOutgoing.Add(senderId.Value))

            {

                tasks.Add(

                    _hub.Clients.User(senderId.Value.ToString())

                        .SendAsync("OutgoingNotification", notification)

                );

            }


            foreach (var userId in bellUsers)

            {

                if (sentBell.Add(userId))

                {

                    tasks.Add(

                        _hub.Clients.User(userId.ToString())

                            .SendAsync("BellNotification", notification)

                    );

                }

            }

            if (evt.TenantId > 0)

            {

                string tenantGroup = $"tenant-{evt.TenantId}";

                tasks.Add(

                    _hub.Clients.Group(tenantGroup)

                        .SendAsync("TenantNotification", notification)

                );

            }

            try

            {

                await Task.WhenAll(tasks);

            }

            catch (Exception ex)

            {

                _logger.LogError(ex, "Failed to dispatch WorkOrderProgressUpdated for Tenant {TenantId}", evt.TenantId);

            }

        }

        //The three dispatcher components below need to be updated for the new system(incoming, outgoing, and bell notifications).

        public async Task DispatchPurchaseRequestAsync(InventoryEventDto evt)
        {
            var tasks = new List<Task>();

            if (evt.TargetUserIds != null)
            {
                foreach (var userId in evt.TargetUserIds)
                {
                    if (userId.HasValue)
                    {
                        tasks.Add(
                            _hub.Clients.User(userId.Value.ToString())
                                .SendAsync("PurchaseRequisitionCreated", evt)
                        );
                    }
                }
            }

            await Task.WhenAll(tasks);
        }
        public async Task DispatchAssetLocationStatusChangeAsync(AssetEventDto evt)
        {
            var tasks = new List<Task>();

            if (evt.TargetUserId.HasValue)
            {
                tasks.Add(
                    _hub.Clients.User(evt.TargetUserId.Value.ToString())
                        .SendAsync("AssetLocationStatusChanged", evt)
                );
            }

            await Task.WhenAll(tasks);
        }
        public async Task DispatchUpdatePurchaseRequestAsync(InventoryEventDto evt)
        {
            var tasks = new List<Task>();

            if (evt.TargetUserIds != null)
            {
                foreach (var userId in evt.TargetUserIds)
                {
                    if (userId.HasValue)
                    {
                        tasks.Add(
                            _hub.Clients.User(userId.Value.ToString())
                                .SendAsync("UpdatePurchaseRequest", evt)
                        );
                    }
                }
            }

            await Task.WhenAll(tasks);
        }

        //End

        //Helper message builders
        private string BuildWorkOrderMessage(WorkOrderProgressUpdatedEventDto evt)
        {
            var technician = evt.TechnicianName ?? "User";
            var title = evt.Title ?? "";
            var workOrderNumber = string.IsNullOrEmpty(evt.WorkOrderNumber)
                ? evt.WorkOrderId.ToString()
                : evt.WorkOrderNumber;

            var statusText = evt.Status?.Trim();

            if (string.IsNullOrEmpty(statusText))
                return $"Work Order #{workOrderNumber} – '{title}' updated.";

            if (!Enum.TryParse<WorkOrderStatus>(statusText, true, out var status))
                return $"Work Order #{workOrderNumber} – '{title}' updated.";

            return status switch
            {
                WorkOrderStatus.Started =>
                    $"Work Order #{workOrderNumber} – '{title}' has been started by {technician}.",

                WorkOrderStatus.InProgress =>
                    $"Work Order #{workOrderNumber} – '{title}' is in progress by {technician}.",

                WorkOrderStatus.OnHold =>
                    $"Work Order #{workOrderNumber} – '{title}' is on hold.",

                WorkOrderStatus.Completed =>
                    $"Work Order #{workOrderNumber} – '{title}' has been completed by {technician}.",

                WorkOrderStatus.Cancelled =>
                    $"Work Order #{workOrderNumber} – '{title}' has been cancelled.",

                WorkOrderStatus.Overdue =>
                    $"Work Order #{workOrderNumber} – '{title}' is overdue.",

                WorkOrderStatus.Closed =>
                    $"Work Order #{workOrderNumber} – '{title}' has been closed.",

                WorkOrderStatus.ReOpened =>
                    $"Work Order #{workOrderNumber} – '{title}' has been reopened.",

                _ =>
                    $"Work Order #{workOrderNumber} – '{title}' updated."
            };
        }
        private string BuildLowStockMessage(LowStockEventDto evt, string eventType)
        {
            var itemName = string.IsNullOrWhiteSpace(evt.ItemName)
                ? "Item"
                : evt.ItemName;

            var itemCode = string.IsNullOrWhiteSpace(evt.ItemCode)
                ? ""
                : $" ({evt.ItemCode})";

            var quantity = evt.QuantityAvailable;

            return eventType switch
            {
                "Warning" =>
                    $" {itemName}{itemCode} is running low. Available: {quantity}.",

                "Error" =>
                    $" {itemName}{itemCode} is OUT OF STOCK! Available: {quantity}.",

                _ =>
                    $"Stock Update: {itemName}{itemCode} available quantity is {quantity}."
            };
        }
        private string BuildWorkOrderEventMessage(WorkOrderEventDto evt)
        {
            var title = evt.Title ?? "";
            var workOrderNumber = string.IsNullOrEmpty(evt.WorkOrderNumber)
                ? evt.WorkOrderId.ToString()
                : evt.WorkOrderNumber;

            return evt.EventType switch
            {
                "Created" =>
                    $"Work Order #{workOrderNumber} – '{title}' has been created.",

                "Updated" =>
                    $"Work Order #{workOrderNumber} – '{title}' has been updated.",

                "Assigned" =>
                    $"Work Order #{workOrderNumber} – '{title}' has been assigned.",

                "Deleted" =>
                    $"Work Order #{workOrderNumber} – '{title}' has been deleted.",

                "WorkOrderApproved" =>
                    $"Work Order #{workOrderNumber} – '{title}' has been approved.",

                "WorkOrderRejected" =>
                    $"Work Order #{workOrderNumber} – '{title}' has been rejected and reopened.",

                "Approved" =>
                   $"Work Order #{workOrderNumber} – '{title}' has been approved.",

                "Rejected" =>
                    $"Work Order #{workOrderNumber} – '{title}' has been rejected.",


                "DueReminder" =>
                    $"Work Order #{workOrderNumber} – '{title}' is due tomorrow.",

                "Overdue" =>
                    $"Work Order #{workOrderNumber} – '{title}' is overdue.",

                "ApprovalReminder" =>
                    $"Work Order {evt.WorkOrderNumber} is waiting for approval/rejection",

                _ =>
                    $"Work Order #{workOrderNumber} – '{title}' updated."
            };
        }
        private string BuildServiceRequestMessage(ServiceRequestEventDto evt)
        {
            var title = evt.Title ?? "";
            var srNumber = string.IsNullOrEmpty(evt.ServiceRequestNumber)
                ? evt.ServiceRequestId?.ToString()
                : evt.ServiceRequestNumber;

            return evt.EventType switch
            {
                "Created" =>
                    $"Service Request #{srNumber} – '{title}' has been created.",

                "Assigned" =>
                    $"Work Order #{evt.WorkOrderNumber} for Service Request #{srNumber} has been assigned to {evt.AssignedTo}.",

                "Reopened" =>
                    $"Service Request #{srNumber} – '{title}' has been reopened.",

                "Approved" =>
                    $"Service Request #{srNumber} – '{title}' has been approved and converted to Work Order #{evt.WorkOrderNumber}.",

                "Rejected" =>
                    $"Service Request #{srNumber} – '{title}' has been rejected. Reason: {evt.Reason}",

                _ =>
                    $"Service Request #{srNumber} – '{title}' updated."
            };
        }

    }
}
