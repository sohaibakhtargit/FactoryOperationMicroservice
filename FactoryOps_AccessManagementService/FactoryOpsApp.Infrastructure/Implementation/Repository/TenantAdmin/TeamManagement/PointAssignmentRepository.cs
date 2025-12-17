using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Repositories.TenantAdmin.TeamManagement;
using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Services.SuperAdmin.AuditLogs;
using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.ExceptionLogger;
using FactoryOps_AccessManagementService.FactoryOpsApp.Application.Common;
using FactoryOpsApp.Domain.Entities.FactoryOpsTenants;
using FactoryOpsApp.Infrastructure.DBContext;
using Microsoft.EntityFrameworkCore;
using static FactoryOps_AccessManagementService.FactoryOpsApp.Common.CommonConstant;

namespace FactoryOperation_AccessManagementService.FactoryOpsApp.Infrastructure.Implementation.Repository.TenantAdmin.TeamManagement
{
    public class PointAssignmentRepository : IPointAssignmentRepository
    {
        private readonly TenantDbContextFactory _tenantDbContext;
        private readonly IAuditLogService _auditLogger;
        private readonly IExceptionLoggerService _exceptionLogger;

        public PointAssignmentRepository(TenantDbContextFactory tenantDbContext,
                                       IAuditLogService auditLogger,
                                       IExceptionLoggerService exceptionLogger)
        {
            _tenantDbContext = tenantDbContext;
            _auditLogger = auditLogger;
            _exceptionLogger = exceptionLogger;
        }

        public async Task<CommonResponseModel> AddPointAssignmentAsync(PointAssignmentDto dto)
        {
            try
            {
                using var tenantDb = _tenantDbContext.GetTenantDbContext(dto.TenantId);
                int basePoints = CalculateBasePoints(dto.Complexity);
                int totalPoints = basePoints + dto.BonusPoints;

                var entity = new PointAssignment
                {
                    TaskName = dto.TaskName,
                    AssignedToUserId = dto.AssignedToUserId,
                    TeamId = dto.TeamId,
                    Complexity = dto.Complexity,
                    Urgency = dto.Urgency,
                    BasePoints = basePoints,
                    BonusPoints = dto.BonusPoints,
                    TotalPoints = totalPoints,
                    Status = dto.Status,
                    CompletionDate = dto.CompletionDate,
                    TenantId = dto.TenantId,
                    IsActive = dto.IsActive,
                    CreatedAt = DateTime.UtcNow,
                    Description = dto.Description
                };

                await tenantDb.PointAssignments.AddAsync(entity);
                await tenantDb.SaveChangesAsync();

                await _auditLogger.LogAuditAsync("Create", $"Created point assignment '{entity.TaskName}' with {totalPoints} points", dto.TenantId, "", "AddPointAssignmentAsync");

                return new CommonResponseModel { StatusCode = StatusCode.Success, StatusMessage = PointAssignmentStatusMessage.PointAssignmentAdded };
            }
            catch (Exception ex)
            {
                await _exceptionLogger.LogExceptionAsync(ex, "PointAssignment-Module", "AddPointAssignmentAsync", dto.TenantId, null);
                return new CommonResponseModel { StatusCode = StatusCode.Error, StatusMessage = $"{PointAssignmentStatusMessage.PointAssignmentAddFailed}: {ex.Message}" };
            }
        }
        private int CalculateBasePoints(string complexity)
        {
            return complexity?.ToLower() switch
            {
                "low" => 50,
                "medium" => 100,
                "high" => 200,
                _ => 50 // Default to low if not specified
            };
        }

        public async Task<CommonResponseModel> UpdatePointAssignmentAsync(PointAssignmentDto dto)
        {
            try
            {
                using var tenantDb = _tenantDbContext.GetTenantDbContext(dto.TenantId);

                var entity = await tenantDb.PointAssignments
                    .FirstOrDefaultAsync(pa => pa.PointAssignmentId == dto.PointAssignmentId &&
                                             pa.TenantId == dto.TenantId &&
                                             pa.IsActive);

                if (entity == null)
                    return new CommonResponseModel { StatusCode = StatusCode.NotFound, StatusMessage = PointAssignmentStatusMessage.PointAssignmentNotFound };

                // Recalculate base points if complexity changed
                int basePoints = CalculateBasePoints(dto.Complexity);
                int totalPoints = basePoints + dto.BonusPoints;

                entity.TaskName = dto.TaskName;
                entity.AssignedToUserId = dto.AssignedToUserId;
                entity.TeamId = dto.TeamId;
                entity.Complexity = dto.Complexity;
                entity.Urgency = dto.Urgency;
                entity.BasePoints = basePoints;
                entity.BonusPoints = dto.BonusPoints;
                entity.TotalPoints = totalPoints;
                entity.Status = dto.Status;
                entity.CompletionDate = dto.CompletionDate;
                entity.IsActive = dto.IsActive;
                entity.Description = dto.Description;

                await tenantDb.SaveChangesAsync();

                await _auditLogger.LogAuditAsync("Update", $"Updated point assignment '{entity.TaskName}' with {totalPoints} points", dto.TenantId, "", "UpdatePointAssignmentAsync");

                return new CommonResponseModel { StatusCode = StatusCode.Success, StatusMessage = PointAssignmentStatusMessage.PointAssignmentUpdated };
            }
            catch (Exception ex)
            {
                await _exceptionLogger.LogExceptionAsync(ex, "PointAssignment-Module", "UpdatePointAssignmentAsync", dto.TenantId, null);
                return new CommonResponseModel { StatusCode = StatusCode.Error, StatusMessage = $"{PointAssignmentStatusMessage.PointAssignmentUpdateFailed} : {ex.Message}" };
            }
        }

        public async Task<CommonResponseModel> DeletePointAssignmentAsync(int pointAssignmentId, int tenantId)
        {
            try
            {
                using var tenantDb = _tenantDbContext.GetTenantDbContext(tenantId);

                var entity = await tenantDb.PointAssignments
                    .FirstOrDefaultAsync(pa => pa.PointAssignmentId == pointAssignmentId &&
                                             pa.TenantId == tenantId &&
                                             pa.IsActive);

                if (entity == null)
                    return new CommonResponseModel { StatusCode = StatusCode.NotFound, StatusMessage = PointAssignmentStatusMessage.PointAssignmentNotFound };

                entity.IsActive = false;

                await tenantDb.SaveChangesAsync();

                await _auditLogger.LogAuditAsync("Delete", $"Deleted point assignment '{entity.TaskName}'", tenantId, "", "DeletePointAssignmentAsync");

                return new CommonResponseModel { StatusCode = StatusCode.Success, StatusMessage = PointAssignmentStatusMessage.PointAssignmentDeleted };
            }
            catch (Exception ex)
            {
                await _exceptionLogger.LogExceptionAsync(ex, "PointAssignment-Module", "DeletePointAssignmentAsync", tenantId, null);
                return new CommonResponseModel { StatusCode = StatusCode.Error, StatusMessage = $"{PointAssignmentStatusMessage.PointAssignmentDeleteFailed} : : {ex.Message}" };
            }
        }

        public async Task<GetAllRecord<GetPointAssignmentDto>> GetAllPointAssignmentsAsync(int tenantId)
        {
            try
            {
                using var tenantDb = _tenantDbContext.GetTenantDbContext(tenantId);

                var entities = await tenantDb.PointAssignments
                    .Include(pa => pa.Team)
                    // Removed Include for AssignedToUser since FK is removed
                    .Where(pa => pa.TenantId == tenantId && pa.IsActive)
                    .OrderByDescending(pa => pa.CreatedAt)
                    .ToListAsync();

                // Get all user IDs from point assignments
                var userIds = entities.Select(pa => pa.AssignedToUserId).Distinct().ToList();

                // Fetch users separately
                var users = await tenantDb.FactoryUsers
                    .Where(u => userIds.Contains(u.UserId))
                    .ToDictionaryAsync(u => u.UserId, u => u);

                var dtoList = entities.Select(pa =>
                {
                    // Get user from dictionary or null if not found
                    users.TryGetValue(pa.AssignedToUserId, out var assignedUser);

                    return new GetPointAssignmentDto
                    {
                        PointAssignmentId = pa.PointAssignmentId,
                        TaskName = pa.TaskName,
                        AssignedToUserId = pa.AssignedToUserId,
                        TeamId = pa.TeamId,
                        Complexity = pa.Complexity,
                        Urgency = pa.Urgency,
                        BasePoints = pa.BasePoints,
                        BonusPoints = pa.BonusPoints,
                        TotalPoints = pa.TotalPoints,
                        Status = pa.Status,
                        CompletionDate = pa.CompletionDate,
                        TenantId = pa.TenantId,
                        IsActive = pa.IsActive,
                        TeamName = pa.Team != null ? pa.Team.Name : "No Team",
                        // Get user name from separately fetched users
                        AssignedToUserName = assignedUser != null ?
                            $"{assignedUser.FirstName} {assignedUser.LastName}" : "Unknown User",
                        StatusName = pa.Status.ToString(), // Use enum name directly
                        CreatedAt = pa.CreatedAt,
                        Description = pa.Description
                    };
                }).ToList();

                return new GetAllRecord<GetPointAssignmentDto>
                {
                    StatusCode = StatusCode.Success,
                    StatusMessage = PointAssignmentStatusMessage.PointAssignmentsFetched,
                    GetAllData = dtoList
                };
            }
            catch (Exception ex)
            {
                await _exceptionLogger.LogExceptionAsync(ex, "PointAssignment-Module", "GetAllPointAssignmentsAsync", tenantId, null);
                return new GetAllRecord<GetPointAssignmentDto>
                {
                    StatusCode = StatusCode.Error,
                    StatusMessage = $"{PointAssignmentStatusMessage.PointAssignmentsFetchFailed}: {ex.Message}"
                };
            }
        }

        public async Task<GetSpecificRecord<GetPointAssignmentDto>> GetPointAssignmentByIdAsync(int pointAssignmentId, int tenantId)
        {
            try
            {
                using var tenantDb = _tenantDbContext.GetTenantDbContext(tenantId);

                var entity = await tenantDb.PointAssignments
                    .Include(pa => pa.Team)
                    // Removed Include for AssignedToUser since FK is removed
                    .FirstOrDefaultAsync(pa => pa.PointAssignmentId == pointAssignmentId &&
                                            pa.TenantId == tenantId &&
                                            pa.IsActive);

                if (entity == null)
                    return new GetSpecificRecord<GetPointAssignmentDto>
                    {
                        StatusCode = StatusCode.NotFound,
                        StatusMessage = PointAssignmentStatusMessage.PointAssignmentNotFound,
                    };

                // Fetch the assigned user separately
                var assignedUser = await tenantDb.FactoryUsers
                    .FirstOrDefaultAsync(u => u.UserId == entity.AssignedToUserId);

                var dto = new GetPointAssignmentDto
                {
                    PointAssignmentId = entity.PointAssignmentId,
                    TaskName = entity.TaskName,
                    AssignedToUserId = entity.AssignedToUserId,
                    TeamId = entity.TeamId,
                    Complexity = entity.Complexity,
                    Urgency = entity.Urgency,
                    BasePoints = entity.BasePoints,
                    BonusPoints = entity.BonusPoints,
                    TotalPoints = entity.TotalPoints,
                    Status = entity.Status,
                    CompletionDate = entity.CompletionDate,
                    TenantId = entity.TenantId,
                    IsActive = entity.IsActive,
                    TeamName = entity.Team != null ? entity.Team.Name : "No Team",
                    // Get user name from separately fetched user
                    AssignedToUserName = assignedUser != null ?
                        $"{assignedUser.FirstName} {assignedUser.LastName}" : "Unknown User",
                    StatusName = entity.Status.ToString(), // Use enum name directly
                    CreatedAt = entity.CreatedAt,
                    Description = entity.Description
                };

                return new GetSpecificRecord<GetPointAssignmentDto>
                {
                    StatusCode = StatusCode.Success,
                    StatusMessage = PointAssignmentStatusMessage.PointAssignmentsFetched,
                    Data = dto
                };
            }
            catch (Exception ex)
            {
                await _exceptionLogger.LogExceptionAsync(ex, "PointAssignment-Module", "GetPointAssignmentByIdAsync", tenantId, null);
                return new GetSpecificRecord<GetPointAssignmentDto>
                {
                    StatusCode = StatusCode.Error,
                    StatusMessage = $"{PointAssignmentStatusMessage.PointAssignmentsFetchFailed}: {ex.Message}"
                };
            }
        }


    }
}