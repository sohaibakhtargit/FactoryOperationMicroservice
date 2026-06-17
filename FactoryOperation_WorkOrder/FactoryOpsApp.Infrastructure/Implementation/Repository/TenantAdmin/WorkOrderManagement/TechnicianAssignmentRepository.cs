using FactoryOperation_WorkOrder.FactoryOpsApp.Application.DTOs;
using FactoryOperation_WorkOrder.FactoryOpsApp.Application.Interfaces.Repositories.TenantAdmin.WorkOrderManagement;
using FactoryOperation_WorkOrder.FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.AuditLogs;
using FactoryOperation_WorkOrder.FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.Common;
using FactoryOperation_WorkOrder.FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.Notification;
using FactoryOperation_WorkOrder.FactoryOpsApp.Infrastructure.Implementation.Services.TenantAdmin.WorkOrderManagement;
using FactoryOpsApp.Application.Common;
using FactoryOpsApp.Application.DTOs;
using FactoryOpsApp.Domain.Entities.FactoryOpsTenants;
using FactoryOpsApp.Infrastructure.DBContext;
using Microsoft.EntityFrameworkCore;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using System.Text;
using System.Text.Json;
using static FactoryOperation_WorkOrder.FactoryOpsApp.Common.CommonConstantURLs;
using static FactoryOpsApp.Application.DTOs.WorkOrderCreateDto;
using static FactoryOpsApp.Common.CommonConstant;

namespace FactoryOperation_WorkOrder.FactoryOpsApp.Infrastructure.Implementation.Repository.TenantAdmin.WorkOrderManagement
{
    public class TechnicianAssignmentRepository : ITechnicianAssignmentRepository
    {
        private readonly MasterFactoryOpsDbContext _masterDbcontext;
        private readonly TenantDbContextFactory _tenantDbContext;
        private readonly IAuditLogService _auditLogger;
        private readonly INotificationService _notificationService;
        private readonly INotificationBuilderService _notificationBuilderService;
        public TechnicianAssignmentRepository(
            MasterFactoryOpsDbContext masterDbcontext,
            TenantDbContextFactory tenantDbContext,
            IAuditLogService auditLogger,
            INotificationService notificationService,
            INotificationBuilderService notificationBuilderService)
        {
            _masterDbcontext = masterDbcontext;
            _tenantDbContext = tenantDbContext;
            _auditLogger = auditLogger;
            _notificationService = notificationService;
            _notificationBuilderService = notificationBuilderService;
        }

        public async Task<GetAllRecord<TechnicianAssignment_DispatchDto>> GetTechnicianDashboardSummaryAsync(int tenantId)
        {
           
                using var tenantDb = _tenantDbContext.GetTenantDbContext(tenantId);

                var totalTechnicians = await tenantDb.FactoryUsers
               .CountAsync(u => u.TenantId == tenantId && u.IsActive && !u.IsDeleted);

                var availableTechnicians = await tenantDb.FactoryUsers
              .CountAsync(u => u.TenantId == tenantId && u.IsActive && !u.Suspend && !u.IsDeleted);


                var AsignedWorkOrders = await tenantDb.WorkOrders
                .CountAsync(w => w.TenantId == tenantId
                              && w.Status == WorkOrderStatus.Assigned
                              && !w.IsDeleted);
                var inProgressWorkOrders = await tenantDb.WorkOrders
             .CountAsync(w => w.TenantId == tenantId
                           && w.Status == WorkOrderStatus.InProgress
                           && !w.IsDeleted);
                var unassignedWorkOrders = await tenantDb.WorkOrders
                .CountAsync(w => w.TenantId == tenantId && !w.IsDeleted && w.AssignedToUserId == null || w.Status == WorkOrderStatus.Pending);

                var dto = new TechnicianAssignment_DispatchDto
                {
                    TenantId = tenantId,
                    AvailableTechnicians = availableTechnicians,
                    TotalTechnicians = totalTechnicians,
                    UnassignedWorkOrders = unassignedWorkOrders,
                    InProgressWorkOrders = inProgressWorkOrders,
                    AsignedWorkOrders = AsignedWorkOrders,

                };
                if (totalTechnicians == 0 && AsignedWorkOrders == 0 && inProgressWorkOrders == 0 && unassignedWorkOrders == 0)
                {
                    return new GetAllRecord<TechnicianAssignment_DispatchDto>
                    {
                        StatusCode = StatusCode.NotFound,
                        StatusMessage = TechnicianAssignmentStatusMessage.TechnicianDashboardSummaryNotFound,
                        GetAllData = new List<TechnicianAssignment_DispatchDto>()
                    };
                }
                else
                {
                    return new GetAllRecord<TechnicianAssignment_DispatchDto>
                    {
                        StatusCode = StatusCode.Success,
                        StatusMessage = TechnicianAssignmentStatusMessage.TechnicianDashboardSummaryFetched,
                        GetAllData = new List<TechnicianAssignment_DispatchDto> { dto }
                    };
                }
            }

        public async Task<GetAllRecord<WorkOrdersRequiringAssignmentDto>> GetTechnicianWorkOrdersAsync(int tenantId)
        {
                using var tenantDb = _tenantDbContext.GetTenantDbContext(tenantId);
                var workOrders = await tenantDb.WorkOrders
                .Where(w => w.TenantId == tenantId && w.IsActive && !w.IsDeleted)
                .Include(w => w.Location)
                .Include(w => w.AssignedToUser)
                  .OrderBy(w => w.Priority == PriorityLevel.Critical ? 1 :
                              w.Priority == PriorityLevel.High ? 2 :
                              w.Priority == PriorityLevel.Medium ? 3 :
                              w.Priority == PriorityLevel.Low ? 4 : 5)
                .Select(w => new WorkOrdersRequiringAssignmentDto
               
                {
                    TenantId = tenantId,
                    WorkOrderId = w.WorkOrderId,
                    WorkOrderNumber = w.WorkOrderNumber,
                    Title = w.Title,
                    Priority = w.Priority.ToString()!.ToLower(),
                    LocationId = w.LocationId,
                    Location = w.Location != null ? $"{w.Location.LocationName}" : "N/A",
                    TechId = w.AssignedToUserId,
                    TechName = w.AssignedToUser != null ? w.AssignedToUser.FirstName + " " + w.AssignedToUser.LastName : "Unassigned",
                    Status = w.Status
                })
                .ToListAsync();

                return new GetAllRecord<WorkOrdersRequiringAssignmentDto>
                {
                    StatusCode = StatusCode.Success,
                    StatusMessage = TechnicianAssignmentStatusMessage.WorkOrdersFetched,
                    GetAllData = workOrders
                };
            }
           

        public async Task<GetAllRecord<TechnicianDto>> GetTechniciansAsync(int tenantId)
        {
            using var tenantDb = _tenantDbContext.GetTenantDbContext(tenantId);

                var TechnicianData = await tenantDb.FactoryUsers
                    .Where(u => u.TenantId == tenantId && u.IsActive && !u.IsDeleted)
                    .Select(u => new TechnicianDto
                    {
                        TenantId = tenantId,
                        UserId = u.UserId,
                        FullName = u.FirstName + " " + u.LastName,
                        ContactNumber = u.ContactNumber,
                        Email = u.Email,
                        ActiveWorkOrders = tenantDb.WorkOrders
                        .Count(w => w.AssignedToUserId == u.UserId
                             && !w.IsDeleted
                             && (w.Status == WorkOrderStatus.InProgress
                                 || w.Status == WorkOrderStatus.Assigned))

                    }).ToListAsync();

                return new GetAllRecord<TechnicianDto>
                {
                    StatusCode = StatusCode.Success,
                    StatusMessage = TechnicianAssignmentStatusMessage.TechniciansFetched,
                    GetAllData = TechnicianData
                };
            }


        /*        public async Task<CommonResponseModel> AssignTechnicianAsync(AssignTechnicianUpdateWorkOrder dto)
                {
                    var response = new CommonResponseModel();
                    using var tenantDb = _tenantDbContext.GetTenantDbContext(dto.TenantId);

                        var entity = await tenantDb.WorkOrders
                            .FirstOrDefaultAsync(x => x.WorkOrderId == dto.WorkOrderId && !x.IsDeleted);
                        if (entity == null)
                        {
                            response.StatusCode = StatusCode.NotFound;
                            response.StatusMessage = TechnicianAssignmentStatusMessage.WorkOrderNotFound;
                            return response;
                        }

                        entity.AssignedToTeamId = dto.TeamId;
                        entity.AssignedToUserId = dto.UserId;
                        entity.Status = dto.Status;
                        entity.UpdatedBy = dto.UpdatedBy; 
                        entity.UpdatedAt = DateTime.UtcNow;

                        await tenantDb.SaveChangesAsync();

                        var notificationEntity = new MasterNotification
                        {
                            TenantId = entity.TenantId,
                            Module = "WorkOrder",
                            EntityId = entity.WorkOrderId,
                            Title = $"Work order assigned: {entity.Title}",
                            Message = $"Work order '{entity.Title}' has been assigned to {entity.AssignedToUser}.",
                            NotificationType = "Assignment",
                            TargetUserId = entity.AssignedToUserId,
                            TargetTeamId = entity.AssignedToTeamId,
                            CreatedByUserId = dto.UpdatedBy,
                            CreatedAt = DateTime.UtcNow,
                            IsRead = false,
                            AdditionalData = JsonDocument.Parse(JsonSerializer.Serialize(new
                            {
                                entity.WorkOrderNumber,
                                Priority = entity.Priority.ToString(),
                                Status = entity.Status.ToString()
                            }))
                        };

                        tenantDb.MasterNotifications.Add(notificationEntity);
                        await tenantDb.SaveChangesAsync();

                        _ = Task.Run(() => _notificationService.NotifyWorkOrderAssignmentAsync(new WorkOrderNotificationDto
                        {
                            EventType = "Assigned",
                            WorkOrderId = entity.WorkOrderId,
                            WorkOrderNumber = entity.WorkOrderNumber,
                            Title = entity.Title,
                            Message = $"Work order '{entity.Title}' has been assigned to team ID {entity.AssignedToTeamId}",
                            EventTime = DateTime.UtcNow,
                            TenantId = entity.TenantId,
                            CreatedByUserId = dto.UpdatedBy,
                            AssignedToUserId = entity.AssignedToUserId,
                            AssignedToTeamId = entity.AssignedToTeamId,
                            Priority = entity.Priority.ToString(),
                            Status = entity.Status.ToString()
                        }));

                        await _auditLogger.LogAuditAsync(
                            "WorkOrder",
                            "Update",
                            dto.UpdatedBy,
                            dto.TenantId.ToString(),
                            entity.WorkOrderId.ToString()
                        );

                        response.StatusCode = StatusCode.Success;
                        response.StatusMessage = TechnicianAssignmentStatusMessage.TechnicianAssignmentUpdated;

                    return response;
                }
        */



        public async Task<CommonResponseModel> AssignTechnicianAsync(AssignTechnicianUpdateWorkOrder dto)
        {
            var response = new CommonResponseModel();

            using var tenantDb = _tenantDbContext.GetTenantDbContext(dto.TenantId);
            using var transaction = await tenantDb.Database.BeginTransactionAsync();

            var entity = await tenantDb.WorkOrders
                .FirstOrDefaultAsync(x => x.WorkOrderId == dto.WorkOrderId && !x.IsDeleted);

            if (entity == null)
            {
                response.StatusCode = StatusCode.NotFound;
                response.StatusMessage = TechnicianAssignmentStatusMessage.WorkOrderNotFound;
                return response;
            }
            var oldAssignedUserId = entity.AssignedToUserId;

            entity.AssignedToTeamId = dto.TeamId;
            entity.AssignedToUserId = dto.UserId;
           // entity.Status = dto.Status;
            entity.UpdatedBy = dto.UpdatedBy;
            entity.UpdatedAt = DateTime.UtcNow;


            if (oldAssignedUserId == dto.UserId)
            {
                await transaction.CommitAsync();

                response.StatusCode = StatusCode.Success;
                response.StatusMessage = TechnicianAssignmentStatusMessage.TechnicianAssignmentUpdated;

                return response;
            }

            if (entity.ServiceRequestId.HasValue &&
                dto.UserId.HasValue &&
                oldAssignedUserId != dto.UserId)
            {
                var serviceRequest = await tenantDb.ServiceRequests
                    .FirstOrDefaultAsync(x =>
                        x.ServiceRequestId == entity.ServiceRequestId &&
                        x.TenantId == dto.TenantId &&
                        !x.IsDeleted);

                if (serviceRequest != null)
                {
                    serviceRequest.AssignedToUserId = dto.UserId;
                    serviceRequest.Status = ServiceRequestStatus.Assigned;
                    serviceRequest.UpdatedAt = DateTime.UtcNow;
                    serviceRequest.UpdatedBy = dto.UpdatedBy;
                }
            }

            await tenantDb.SaveChangesAsync();


            if (entity.AssignedToUserId.HasValue)
            {

                //var assignedAssetUserId = await tenantDb.AssetRegistry
                //    .Where(x => x.AssetId == entity.AssetId && x.IsActive && !x.IsDeleted)
                //    .Join(
                //        tenantDb.AssetTracking,
                //        assetRegistry => assetRegistry.AssetId,
                //        assetTracking => assetTracking.AssetId,
                //        (assetRegistry, assetTracking) => assetTracking
                //    )
                //    .Where(x => !x.IsDeleted)
                //    .Select(x => x.AssignedTo)
                //    .FirstOrDefaultAsync();

                //var teamUserIds = new List<int>();

                //if (entity.AssignedToTeamId.HasValue)
                //{
                //    teamUserIds = await tenantDb.FactoryTeamMembers
                //        .Where(x => x.TeamId == entity.AssignedToTeamId &&
                //                    x.IsActive &&
                //                    !x.IsDeleted &&
                //                    x.UserId.HasValue)
                //        .Select(x => x.UserId!.Value)
                //        .ToListAsync();
                //}

                //var maintenanceSupervisorRoleId = await tenantDb.FactoryRoles
                //    .Where(x => x.RoleName == "Maintenance Supervisor" && !x.IsDeleted)
                //    .Select(x => x.RoleId)
                //    .FirstOrDefaultAsync();

                //var productionSupervisorRoleId = await tenantDb.FactoryRoles
                //    .Where(x => x.RoleName == "Production Supervisor" && !x.IsDeleted)
                //    .Select(x => x.RoleId)
                //    .FirstOrDefaultAsync();

                //var maintenanceSupervisorUserIds = await tenantDb.FactoryUserRoles
                //    .Where(x => x.TenantId == entity.TenantId &&
                //                x.RoleId == maintenanceSupervisorRoleId &&
                //                !x.IsDeleted &&
                //                teamUserIds.Contains(x.UserId))
                //    .Select(x => x.UserId)
                //    .ToListAsync();

                //var productionSupervisorUserIds = await tenantDb.FactoryUserRoles
                //     .Where(x => x.TenantId == entity.TenantId &&
                //                 x.RoleId == productionSupervisorRoleId &&
                //                 !x.IsDeleted &&
                //                 teamUserIds.Contains(x.UserId))
                //     .Select(x => x.UserId)
                //     .ToListAsync();

                //var CreatedByUserId = await tenantDb.FactoryUsers
                //    .Where(x => x.UserId == entity.CreatedBy && !x.IsDeleted)
                //    .Select(x => x.FirstName + " " + x.LastName)
                //    .FirstOrDefaultAsync();

                //var UpdatedBy = await tenantDb.FactoryUsers
                //    .Where(x => x.UserId == entity.UpdatedBy && !x.IsDeleted)
                //    .Select(x => x.FirstName + " " + x.LastName)
                //    .FirstOrDefaultAsync();

                var TenantEmail = await _masterDbcontext.FactoryTenants
                    .Where(x => x.TenantId == dto.TenantId)
                    .Select(x => x.AdminEmail)
                    .FirstOrDefaultAsync();
                //var AssignedToUserEmail = await tenantDb.FactoryUsers
                //    .Where(x => x.UserId == entity.AssignedToUserId && !x.IsDeleted)
                //    .Select(x => x.Email)
                //    .FirstOrDefaultAsync();

                //var ProductionSupervisorEmails = await tenantDb.FactoryUserRoles
                //     .Where(x => x.TenantId == dto.TenantId &&
                //                 x.RoleId == productionSupervisorRoleId &&
                //                 !x.IsDeleted &&
                //                 teamUserIds.Contains(x.UserId))
                //     .Select(x => x.FactoryUsers.Email)
                //     .ToListAsync();

                //var MaintenanceSupervisorEmails = await tenantDb.FactoryUserRoles
                //    .Where(x => x.TenantId == dto.TenantId &&
                //                x.RoleId == maintenanceSupervisorRoleId &&
                //                !x.IsDeleted &&
                //                teamUserIds.Contains(x.UserId))
                //    .Select(x => x.FactoryUsers.Email)
                //    .ToListAsync();

                //var UpdatedByEmail = await tenantDb.FactoryUsers
                //    .Where(x => x.UserId == entity.UpdatedBy && !x.IsDeleted)
                //    .Select(x => x.Email)
                //    .FirstOrDefaultAsync();
                //var createdByUserId = await tenantDb.WorkOrders
                //     .Where(x => x.WorkOrderId == dto.WorkOrderId)
                //     .Select(x => x.CreatedBy)
                //     .FirstOrDefaultAsync();

                //var outgoingNotifications = dto.UpdatedBy;

                //var incomingNotification = productionSupervisorUserIds
                //    .Concat(maintenanceSupervisorUserIds)
                //   .Append(createdByUserId ?? 0)
                //    .Append(dto.UserId ?? 0)
                //    .Where(x => x != 0 && x != dto.UpdatedBy)
                //    .Distinct()
                //    .Select(x => (int?)x)
                //    .ToList();

                ////if (outgoingNotifications > 0)
                ////{
                ////    incomingNotification = incomingNotification.Where(x => x != outgoingNotifications).ToList();
                ////}

                //var TargetUserIds = incomingNotification
                //    .Append(outgoingNotifications)
                //    .Where(x => x != 0)
                //    .Distinct()
                //    .Select(x => (int?)x)
                //    .ToList();

                //var TargetEmailAddresses = ProductionSupervisorEmails
                //    .Concat(MaintenanceSupervisorEmails)
                //    .Append(AssignedToUserEmail)
                //    .Where(x => !string.IsNullOrEmpty(x) && x != UpdatedByEmail)
                //    .Distinct()
                //    .ToList();

                var assignedToUserIdAsset = await tenantDb.AssetRegistry
                    .Where(x => x.AssetId == entity.AssetId && x.IsActive && !x.IsDeleted)
                    .Join(tenantDb.AssetTracking,
                        a => a.AssetId,
                        t => t.AssetId,
                        (a, t) => t)
                    .Where(x => !x.IsDeleted)
                    .Select(x => x.AssignedTo)
                    .FirstOrDefaultAsync();

                var notificationData = await _notificationBuilderService.BuildAsync(
                                        _tenantDbContext,
                                        entity.TenantId,
                                        dto.UpdatedBy,
                                        entity.AssignedToUserId,
                                        entity.CreatedBy,
                                        assignedToUserIdAsset
                                    );


                /*  supervisorUserIds = supervisorUserIds.Distinct().ToList();*/

                var eventDto = new WorkOrderEventDto
                {
                    WorkOrderId = entity.WorkOrderId,
                    TenantId = entity.TenantId,
                    TenantEmail = TenantEmail,
                    WorkOrderType = entity.WorkOrderType,
                    WorkOrderTypeName = entity.WorkOrderType?.ToString(),
                    WorkOrderNumber = entity.WorkOrderNumber,
                    Title = entity.Title,
                    EventType =  "Assigned",
                    EventTime = DateTime.UtcNow,
                    Priority = entity.Priority.ToString(),
                    Status = entity.Status.ToString(),
                    TargetUserId = entity.AssignedToUserId,
                    AssignedToUserId = entity.AssignedToUserId,
                    incomingNotifications = notificationData.Incoming,
                    outgoingNotifications = notificationData.Outgoing,
                    AssignedToTeamId = entity.AssignedToTeamId,
                    TargetUsersIds = notificationData.AllUsers,
                    TargetEmailAddresses = notificationData.Emails

                };

                var correlationId = Guid.NewGuid().ToString();
                string topicName = KafkaCommonTopics.BuildTopicName(entity.TenantId, eventDto.EventType);

                var kafkaRequest = new
                {
                    Topic = topicName,
                    Key = $"workorder-{entity.WorkOrderId}",
                    Payload = eventDto,
                    Source = "WorkOrderService",
                    Headers = new Dictionary<string, string>
                    {
                        ["tenant-id"] = entity.TenantId.ToString(),
                        ["correlation-id"] = correlationId
                    }
                };

                using (var httpClient = new HttpClient())
                {
                    var jsonContent = new StringContent(
                        JsonSerializer.Serialize(kafkaRequest),
                        Encoding.UTF8,
                        "application/json");

                    var kafkaResponse = await httpClient.PostAsync(ConstantUrls.kafkaPublish, jsonContent);
                    kafkaResponse.EnsureSuccessStatusCode();
                }
            }
            await _auditLogger.LogAuditAsync(
                "WorkOrder",
                "Update",
                dto.UpdatedBy,
                dto.TenantId.ToString(),
                entity.WorkOrderId.ToString()
            );

            await transaction.CommitAsync();

            response.StatusCode = StatusCode.Success;
            response.StatusMessage = TechnicianAssignmentStatusMessage.TechnicianAssignmentUpdated;

            return response;
        }
        public async Task<GetAllRecord<AssignmentHistoryDto>> GetLatestAssignmentHistoryAsync(int tenantId)
        {
         using var tenantDb = _tenantDbContext.GetTenantDbContext(tenantId);

                var history = await tenantDb.WorkOrders
                    .Where(w => w.TenantId == tenantId && !w.IsDeleted && w.AssignedToUserId != null)
                    .Include(w => w.AssignedToUser)
                    .OrderByDescending(w => w.ScheduleDate)
                    .Select(w => new AssignmentHistoryDto
                    {
                        TenantId = tenantId,
                        WorkOrderId = w.WorkOrderId,
                        WorkOrderNumber = w.WorkOrderNumber,
                        Title = w.Title,
                        TechnicianName = w.AssignedToUser != null ? w.AssignedToUser.FirstName + " " + w.AssignedToUser.LastName : "Unassigned",
                        TeamName = w.AssignedToTeam != null ? w.AssignedToTeam.Name + "" : "Unassigned",
                        AssignedDate = w.ScheduleDate ?? DateTime.MinValue,
                        Status = w.Status
                    })
                    .ToListAsync();

            if (!history.Any())
            {
                return new GetAllRecord<AssignmentHistoryDto>
                {
                    StatusCode = StatusCode.NotFound,
                    StatusMessage = TechnicianAssignmentStatusMessage.NoAssignmentHistoryFound,
                    GetAllData = new List<AssignmentHistoryDto>()
                };
            }

            return new GetAllRecord<AssignmentHistoryDto>
                {
                    StatusCode = StatusCode.Success,
                    StatusMessage = TechnicianAssignmentStatusMessage.AssignmentHistoryFetched,
                    GetAllData = history
                };
            }
    }
}
