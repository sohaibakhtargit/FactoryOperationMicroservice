using FactoryOperation_WorkOrder.FactoryOpsApp.Application.Interfaces.Repositories.TenantAdmin.WorkOrderManagement;
using FactoryOperation_WorkOrder.FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.AuditLogs;
using FactoryOperation_WorkOrder.FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.Notification;
using FactoryOpsApp.Application.Common;
using FactoryOpsApp.Application.DTOs;
using FactoryOpsApp.Domain.Entities.FactoryOpsTenants;
using FactoryOpsApp.Infrastructure.DBContext;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using static FactoryOpsApp.Common.CommonConstant;

namespace FactoryOperation_WorkOrder.FactoryOpsApp.Infrastructure.Implementation.Repository.TenantAdmin.WorkOrderManagement
{
    public class TechnicianAssignmentRepository : ITechnicianAssignmentRepository
    {
        private readonly TenantDbContextFactory _tenantDbContext;
        private readonly IAuditLogService _auditLogger;
        private readonly INotificationService _notificationService;

        public TechnicianAssignmentRepository(
            TenantDbContextFactory tenantDbContext,
            IAuditLogService auditLogger,
            INotificationService notificationService)
        {
            _tenantDbContext = tenantDbContext;
            _auditLogger = auditLogger;
            _notificationService = notificationService;
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
                .Select(w => new WorkOrdersRequiringAssignmentDto
                {
                    TenantId = tenantId,
                    WorkOrderId = w.WorkOrderId,
                    WorkOrderNumber = w.WorkOrderNumber,
                    Title = w.Title,
                    Priority = w.Priority.ToString().ToLower(),
                    LocationId = w.LocationId,
                    Location = w.Location != null ? $"{w.Location.LocationName}" : "N/A",
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
           

        public async Task<CommonResponseModel> AssignTechnicianAsync(AssignTechnicianUpdateWorkOrder dto)
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
                    Message = $"Work order '{entity.Title}' has been assigned to team ID {entity.AssignedToTeamId}.",
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

        public async Task<GetAllRecord<AssignmentHistoryDto>> GetLatestAssignmentHistoryAsync(int tenantId)
        {
         using var tenantDb = _tenantDbContext.GetTenantDbContext(tenantId);

                var history = await tenantDb.WorkOrders
                    .Where(w => w.TenantId == tenantId && !w.IsDeleted && w.AssignedToUserId != null)
                    .Include(w => w.AssignedToUser)
                    .OrderByDescending(w => w.ScheduleDate)
                    .Take(4)
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
