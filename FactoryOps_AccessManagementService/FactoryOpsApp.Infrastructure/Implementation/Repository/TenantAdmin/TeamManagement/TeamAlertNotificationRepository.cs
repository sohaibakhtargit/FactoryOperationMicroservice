using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Repositories.TenantAdmin.TeamManagement;
using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Services.SuperAdmin.AuditLogs;
using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.ExceptionLogger;
using FactoryOps_AccessManagementService.FactoryOpsApp.Application.Common;
using FactoryOpsApp.Application.DTOs;
using FactoryOpsApp.Domain.Entities.FactoryOpsTenants;
using FactoryOpsApp.Infrastructure.DBContext;
using Microsoft.EntityFrameworkCore;
using static FactoryOps_AccessManagementService.FactoryOpsApp.Common.CommonConstant;

namespace FactoryOperation_AccessManagementService.FactoryOpsApp.Infrastructure.Implementation.Repository.TenantAdmin.TeamManagement
{
    public class TeamAlertNotificationRepository : ITeamAlertNotificationRepository
    {
        private readonly TenantDbContextFactory _tenantDbContext;
        private readonly IAuditLogService _auditLogger;
        private readonly IExceptionLoggerService _exceptionLogger;

        public TeamAlertNotificationRepository(TenantDbContextFactory tenantDbContext,
                                             IAuditLogService auditLogger,
                                             IExceptionLoggerService exceptionLogger)
        {
            _tenantDbContext = tenantDbContext;
            _auditLogger = auditLogger;
            _exceptionLogger = exceptionLogger;
        }

        public async Task<CommonResponseModel> AddTeamAlertNotificationAsync(CreateTeamAlertNotificationDto dto)
        {
            try
            {
                using var tenantDb = _tenantDbContext.GetTenantDbContext(dto.TenantId);

                var entity = new TeamAlertNotification
                {
                    Title = dto.Title,
                    Message = dto.Message,
                    NotificationType = dto.NotificationType,
                    Priority = 0,
                    TriggerType = dto.TriggerType,
                    ThresholdMinutes = dto.ThresholdMinutes,
                    ConditionValue = dto.ConditionValue,
                    ConditionUnit = dto.ConditionUnit,
                    NotificationChannels = dto.NotificationChannels,
                    IsAlertRule = dto.IsAlertRule,
                    TargetType = dto.TargetType,
                    TargetId = dto.TargetId,
                    SentToUserId = dto.SentToUserId,
                    SentToTeamId = dto.SentToTeamId,
                    SentToAll = dto.SentToAll,
                    TenantId = dto.TenantId,
                    IsActive = true,
                    CreatedBy = dto.CreatedBy,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await tenantDb.TeamAlertNotifications.AddAsync(entity);
                await tenantDb.SaveChangesAsync();

                await _auditLogger.LogAuditAsync("Create", $"Created team alert notification '{entity.Title}'", dto.TenantId, "", "AddTeamAlertNotificationAsync");

                return new CommonResponseModel { StatusCode = StatusCode.Success, StatusMessage = TeamAlertNotificationStatusMessage.TeamAlertNotificationAdded };
            }
            catch (Exception ex)
            {
                await _exceptionLogger.LogExceptionAsync(ex, "TeamAlertNotification-Module", "AddTeamAlertNotificationAsync", dto.TenantId, null);
                return new CommonResponseModel { StatusCode = StatusCode.Error, StatusMessage = $"{TeamAlertNotificationStatusMessage.TeamAlertNotificationAddFailed}: {ex.Message}" };
            }
        }

        public async Task<CommonResponseModel> UpdateTeamAlertNotificationAsync(UpdateTeamAlertNotificationDto dto)
        {
            try
            {
                using var tenantDb = _tenantDbContext.GetTenantDbContext(dto.TenantId);

                var entity = await tenantDb.TeamAlertNotifications
                    .FirstOrDefaultAsync(n => n.TeamAlertNotificationId == dto.TeamAlertNotificationId &&
                                            n.TenantId == dto.TenantId &&
                                            n.IsActive);

                if (entity == null)
                    return new CommonResponseModel { StatusCode = StatusCode.NotFound, StatusMessage = TeamAlertNotificationStatusMessage.TeamAlertNotificationNotFound };

                entity.Title = dto.Title;
                entity.Message = dto.Message;
                entity.NotificationType = dto.NotificationType;
                entity.Priority = dto.Priority;
                entity.TriggerType = dto.TriggerType;
                entity.ThresholdMinutes = dto.ThresholdMinutes;
                entity.ConditionValue = dto.ConditionValue;
                entity.ConditionUnit = dto.ConditionUnit;
                entity.NotificationChannels = dto.NotificationChannels;
                entity.IsAlertRule = dto.IsAlertRule;
                entity.TargetType = dto.TargetType;
                entity.TargetId = dto.TargetId;
                entity.IsRead = dto.IsRead;
                entity.SentToUserId = dto.SentToUserId;
                entity.SentToTeamId = dto.SentToTeamId;
                entity.SentToAll = dto.SentToAll;
                entity.IsActive = dto.IsActive;
                entity.UpdatedBy = dto.UpdatedBy;
                entity.UpdatedAt = DateTime.UtcNow;

                await tenantDb.SaveChangesAsync();

                await _auditLogger.LogAuditAsync("Update", $"Updated team alert notification '{entity.Title}'", dto.TenantId, "", "UpdateTeamAlertNotificationAsync");

                return new CommonResponseModel { StatusCode = StatusCode.Success, StatusMessage = TeamAlertNotificationStatusMessage.TeamAlertNotificationUpdated };
            }
            catch (Exception ex)
            {
                await _exceptionLogger.LogExceptionAsync(ex, "TeamAlertNotification-Module", "UpdateTeamAlertNotificationAsync", dto.TenantId, null);
                return new CommonResponseModel { StatusCode = StatusCode.Error, StatusMessage = $"{TeamAlertNotificationStatusMessage.TeamAlertNotificationUpdateFailed}: {ex.Message}" };
            }
        }

        public async Task<CommonResponseModel> DeleteTeamAlertNotificationAsync(int teamAlertNotificationId, int tenantId)
        {
            try
            {
                using var tenantDb = _tenantDbContext.GetTenantDbContext(tenantId);

                var entity = await tenantDb.TeamAlertNotifications
                    .FirstOrDefaultAsync(n => n.TeamAlertNotificationId == teamAlertNotificationId &&
                                            n.TenantId == tenantId &&
                                            n.IsActive);

                if (entity == null)
                    return new CommonResponseModel { StatusCode = StatusCode.NotFound, StatusMessage = TeamAlertNotificationStatusMessage.TeamAlertNotificationNotFound };

                entity.IsActive = false;
                entity.UpdatedAt = DateTime.UtcNow;

                await tenantDb.SaveChangesAsync();

                await _auditLogger.LogAuditAsync("Delete", $"Deleted team alert notification '{entity.Title}'", tenantId, "", "DeleteTeamAlertNotificationAsync");

                return new CommonResponseModel { StatusCode = StatusCode.Success, StatusMessage = TeamAlertNotificationStatusMessage.TeamAlertNotificationDeleted };
            }
            catch (Exception ex)
            {
                await _exceptionLogger.LogExceptionAsync(ex, "TeamAlertNotification-Module", "DeleteTeamAlertNotificationAsync", tenantId, null);
                return new CommonResponseModel { StatusCode = StatusCode.Error, StatusMessage = $"{TeamAlertNotificationStatusMessage.TeamAlertNotificationDeleteFailed}: {ex.Message}" };
            }
        }

        public async Task<GetAllRecord<GetTeamAlertNotificationDto>> GetAllTeamAlertNotificationsAsync(int tenantId)
        {
            try
            {
                using var tenantDb = _tenantDbContext.GetTenantDbContext(tenantId);

                var entities = await tenantDb.TeamAlertNotifications
                    .Include(n => n.SentToUser)
                    .Include(n => n.SentToTeam)
                    .Where(n => n.TenantId == tenantId && n.IsActive)
                    .OrderByDescending(n => n.CreatedAt)
                    .ToListAsync();

                var dtoList = entities.Select(n => new GetTeamAlertNotificationDto
                {
                    TeamAlertNotificationId = n.TeamAlertNotificationId,
                    Title = n.Title,
                    Message = n.Message,
                    NotificationType = n.NotificationType,
                    Priority = n.Priority,
                    ThresholdMinutes = n.ThresholdMinutes,
                    TriggerType = n.TriggerType,
                    ConditionValue = n.ConditionValue,
                    ConditionUnit = n.ConditionUnit,
                    NotificationChannels = n.NotificationChannels,
                    IsAlertRule = n.IsAlertRule,
                    TargetType = n.TargetType,
                    TargetId = n.TargetId,
                    IsRead = n.IsRead,
                    SentToUserId = n.SentToUserId,
                    SentToTeamId = n.SentToTeamId,
                    SentToAll = n.SentToAll,
                    IsActive = n.IsActive,
                    TenantId = n.TenantId,
                    CreatedAt = n.CreatedAt,
                    UpdatedAt = n.UpdatedAt,
                    SentToUserName = n.SentToUser != null ? $"{n.SentToUser.FirstName} {n.SentToUser.LastName}" : null,
                    SentToTeamName = n.SentToTeam?.Name
                }).ToList();

                return new GetAllRecord<GetTeamAlertNotificationDto>
                {
                    StatusCode = StatusCode.Success,
                    StatusMessage = TeamAlertNotificationStatusMessage.TeamAlertNotificationsFetched,
                    GetAllData = dtoList
                };
            }
            catch (Exception ex)
            {
                await _exceptionLogger.LogExceptionAsync(ex, "TeamAlertNotification-Module", "GetAllTeamAlertNotificationsAsync", tenantId, null);
                return new GetAllRecord<GetTeamAlertNotificationDto>
                {
                    StatusCode = StatusCode.Error,
                    StatusMessage = $"{TeamAlertNotificationStatusMessage.TeamAlertNotificationsFetchFailed}: {ex.Message}"
                };
            }
        }

        public async Task<GetSpecificRecord<GetTeamAlertNotificationDto>> GetTeamAlertNotificationByIdAsync(int teamAlertNotificationId, int tenantId)
        {
            try
            {
                using var tenantDb = _tenantDbContext.GetTenantDbContext(tenantId);

                var entity = await tenantDb.TeamAlertNotifications
                    .Include(n => n.SentToUser)
                    .Include(n => n.SentToTeam)
                    .FirstOrDefaultAsync(n => n.TeamAlertNotificationId == teamAlertNotificationId &&
                                            n.TenantId == tenantId &&
                                            n.IsActive);

                if (entity == null)
                    return new GetSpecificRecord<GetTeamAlertNotificationDto>
                    {
                        StatusCode = StatusCode.NotFound,
                        StatusMessage = TeamAlertNotificationStatusMessage.TeamAlertNotificationNotFound,
                    };

                var dto = new GetTeamAlertNotificationDto
                {
                    TeamAlertNotificationId = entity.TeamAlertNotificationId,
                    Title = entity.Title,
                    Message = entity.Message,
                    NotificationType = entity.NotificationType,
                    Priority = entity.Priority,
                    TriggerType = entity.TriggerType,
                    ThresholdMinutes = entity.ThresholdMinutes,
                    ConditionValue = entity.ConditionValue,
                    ConditionUnit = entity.ConditionUnit,
                    NotificationChannels = entity.NotificationChannels,
                    IsAlertRule = entity.IsAlertRule,
                    TargetType = entity.TargetType,
                    TargetId = entity.TargetId,
                    IsRead = entity.IsRead,
                    SentToUserId = entity.SentToUserId,
                    SentToTeamId = entity.SentToTeamId,
                    SentToAll = entity.SentToAll,
                    IsActive = entity.IsActive,
                    TenantId = entity.TenantId,
                    CreatedAt = entity.CreatedAt,
                    UpdatedAt = entity.UpdatedAt,
                    SentToUserName = entity.SentToUser != null ? $"{entity.SentToUser.FirstName} {entity.SentToUser.LastName}" : null,
                    SentToTeamName = entity.SentToTeam?.Name
                };

                return new GetSpecificRecord<GetTeamAlertNotificationDto>
                {
                    StatusCode = StatusCode.Success,
                    StatusMessage = TeamAlertNotificationStatusMessage.TeamAlertNotificationsFetched,
                    Data = dto
                };
            }
            catch (Exception ex)
            {
                await _exceptionLogger.LogExceptionAsync(ex, "TeamAlertNotification-Module", "GetTeamAlertNotificationByIdAsync", tenantId, null);
                return new GetSpecificRecord<GetTeamAlertNotificationDto>
                {
                    StatusCode = StatusCode.Error,
                    StatusMessage = $"{TeamAlertNotificationStatusMessage.TeamAlertNotificationsFetchFailed}: {ex.Message}"
                };
            }
        }

        public async Task<CommonResponseModel> MarkAsReadAsync(int teamAlertNotificationId, int tenantId)
        {
            try
            {
                using var tenantDb = _tenantDbContext.GetTenantDbContext(tenantId);

                var entity = await tenantDb.TeamAlertNotifications
                    .FirstOrDefaultAsync(n => n.TeamAlertNotificationId == teamAlertNotificationId &&
                                            n.TenantId == tenantId &&
                                            n.IsActive);

                if (entity == null)
                    return new CommonResponseModel { StatusCode = StatusCode.NotFound, StatusMessage = TeamAlertNotificationStatusMessage.TeamAlertNotificationNotFound };

                entity.IsRead = true;
                entity.UpdatedAt = DateTime.UtcNow;

                await tenantDb.SaveChangesAsync();

                await _auditLogger.LogAuditAsync("Update", $"Marked team alert notification '{entity.Title}' as read", tenantId, "", "MarkAsReadAsync");

                return new CommonResponseModel { StatusCode = StatusCode.Success, StatusMessage = TeamAlertNotificationStatusMessage.TeamAlertNotificationMarkedAsRead };
            }
            catch (Exception ex)
            {
                await _exceptionLogger.LogExceptionAsync(ex, "TeamAlertNotification-Module", "MarkAsReadAsync", tenantId, null);
                return new CommonResponseModel { StatusCode = StatusCode.Error, StatusMessage = $"{TeamAlertNotificationStatusMessage.TeamAlertNotificationMarkAsReadFailed}: {ex.Message}" };
            }
        }
    }
}