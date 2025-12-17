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
    public class UserBadgeRepository : IUserBadgeRepository
    {
        private readonly TenantDbContextFactory _tenantDbContext;
        private readonly IAuditLogService _auditLogger;
        private readonly IExceptionLoggerService _exceptionLogger;

        public UserBadgeRepository(TenantDbContextFactory tenantDbContext,
                                 IAuditLogService auditLogger,
                                 IExceptionLoggerService exceptionLogger)
        {
            _tenantDbContext = tenantDbContext;
            _auditLogger = auditLogger;
            _exceptionLogger = exceptionLogger;
        }

        public async Task<CommonResponseModel> AddUserBadgeAsync(UserBadgeDto dto)
        {
            try
            {
                using var tenantDb = _tenantDbContext.GetTenantDbContext(dto.TenantId);

                // Check if user already has this badge
                var existing = await tenantDb.UserBadges
                    .FirstOrDefaultAsync(ub => ub.BadgeId == dto.BadgeId &&
                                             ub.UserId == dto.UserId &&
                                             ub.TenantId == dto.TenantId);

                if (existing != null)
                    return new CommonResponseModel
                    {
                        StatusCode = StatusCode.BadRequest,
                        StatusMessage = UserBadgeStatusMessage.UserAlreadyHasBadge
                    };

                var entity = new UserBadge
                {
                    BadgeId = dto.BadgeId,
                    UserId = dto.UserId,
                    AwardedDate = dto.AwardedDate,
                    Progress = dto.Progress,
                    IsAwarded = dto.IsAwarded,
                    TenantId = dto.TenantId,
                    CreatedAt = DateTime.UtcNow
                };

                await tenantDb.UserBadges.AddAsync(entity);
                await tenantDb.SaveChangesAsync();

                await _auditLogger.LogAuditAsync("Create", $"Added badge to user UserId:{dto.UserId}", dto.TenantId, "", "AddUserBadgeAsync");

                return new CommonResponseModel
                {
                    StatusCode = StatusCode.Success,
                    StatusMessage = UserBadgeStatusMessage.UserBadgeAdded
                };
            }
            catch (Exception ex)
            {
                await _exceptionLogger.LogExceptionAsync(ex, "UserBadge-Module", "AddUserBadgeAsync", dto.TenantId, null);
                return new CommonResponseModel
                {
                    StatusCode = StatusCode.Error,
                    StatusMessage = $"{UserBadgeStatusMessage.UserBadgeAddFailed}: {ex.Message}"
                };
            }
        }

        public async Task<CommonResponseModel> AwardUserBadgeAsync(AwardUserBadgeDto dto)
        {
            try
            {
                using var tenantDb = _tenantDbContext.GetTenantDbContext(dto.TenantId);

                var entity = new UserBadge
                {
                    BadgeId = dto.BadgeId,
                    UserId = dto.UserId,
                    AwardedDate = DateTime.UtcNow,
                    Progress = 100,
                    IsAwarded = true,
                    TenantId = dto.TenantId,
                    CreatedAt = DateTime.UtcNow
                };

                await tenantDb.UserBadges.AddAsync(entity);
                await tenantDb.SaveChangesAsync();

                await _auditLogger.LogAuditAsync("Award", $"Awarded badge to user UserId:{dto.UserId}", dto.TenantId, "", "AwardUserBadgeAsync");

                return new CommonResponseModel
                {
                    StatusCode = StatusCode.Success,
                    StatusMessage = UserBadgeStatusMessage.BadgeAwarded,
                };
            }
            catch (Exception ex)
            {
                await _exceptionLogger.LogExceptionAsync(ex, "UserBadge-Module", "AwardUserBadgeAsync", dto.TenantId, null);
                return new CommonResponseModel
                {
                    StatusCode = StatusCode.Error,
                    StatusMessage = $"{UserBadgeStatusMessage.BadgeAwardFailed}: {ex.Message}"
                };
            }
        }

        public async Task<CommonResponseModel> UpdateUserBadgeProgressAsync(UpdateUserBadgeProgressDto dto)
        {
            try
            {
                using var tenantDb = _tenantDbContext.GetTenantDbContext(dto.TenantId);

                var entity = await tenantDb.UserBadges
                    .FirstOrDefaultAsync(ub => ub.UserBadgeId == dto.UserBadgeId && ub.TenantId == dto.TenantId);

                if (entity == null)
                    return new CommonResponseModel { StatusCode = StatusCode.NotFound, StatusMessage = UserBadgeStatusMessage.UserBadgeNotFound };

                entity.Progress = dto.Progress;
                entity.IsAwarded = dto.IsAwarded;

                if (dto.IsAwarded && !entity.IsAwarded)
                {
                    entity.AwardedDate = DateTime.UtcNow;
                }

                await tenantDb.SaveChangesAsync();

                await _auditLogger.LogAuditAsync("Update", $"Updated badge progress for UserBadgeId:{dto.UserBadgeId}", dto.TenantId, "", "UpdateUserBadgeProgressAsync");

                return new CommonResponseModel
                {
                    StatusCode = StatusCode.Success,
                    StatusMessage = UserBadgeStatusMessage.UserBadgeUpdated
                };
            }
            catch (Exception ex)
            {
                await _exceptionLogger.LogExceptionAsync(ex, "UserBadge-Module", "UpdateUserBadgeProgressAsync", dto.TenantId, null);
                return new CommonResponseModel
                {
                    StatusCode = StatusCode.Error,
                    StatusMessage = $"{UserBadgeStatusMessage.UserBadgeUpdateFailed}: {ex.Message}"
                };
            }
        }

        public async Task<CommonResponseModel> DeleteUserBadgeAsync(int userBadgeId, int tenantId)
        {
            try
            {
                using var tenantDb = _tenantDbContext.GetTenantDbContext(tenantId);

                var entity = await tenantDb.UserBadges
                    .FirstOrDefaultAsync(ub => ub.UserBadgeId == userBadgeId && ub.TenantId == tenantId);

                if (entity == null)
                    return new CommonResponseModel { StatusCode = StatusCode.NotFound, StatusMessage = UserBadgeStatusMessage.UserBadgeNotFound };

                tenantDb.UserBadges.Remove(entity);
                await tenantDb.SaveChangesAsync();

                await _auditLogger.LogAuditAsync("Delete", $"Deleted user badge {userBadgeId}", tenantId, "", "DeleteUserBadgeAsync");

                return new CommonResponseModel
                {
                    StatusCode = StatusCode.Success,
                    StatusMessage = UserBadgeStatusMessage.UserBadgeDeleted
                };
            }
            catch (Exception ex)
            {
                await _exceptionLogger.LogExceptionAsync(ex, "UserBadge-Module", "DeleteUserBadgeAsync", tenantId, null);
                return new CommonResponseModel
                {
                    StatusCode = StatusCode.Error,
                    StatusMessage = $"{UserBadgeStatusMessage.UserBadgeDeleteFailed}: {ex.Message}"
                };
            }
        }

        public async Task<GetAllRecord<GetUserBadgeDto>> GetAllUserBadgesAsync(int tenantId)
        {
            try
            {
                using var tenantDb = _tenantDbContext.GetTenantDbContext(tenantId);

                var entities = await tenantDb.UserBadges
                    .Include(ub => ub.Badge)
                    .Where(ub => ub.TenantId == tenantId)
                    .OrderByDescending(ub => ub.CreatedAt)
                    .ToListAsync();

                var dtoList = entities.Select(ub => new GetUserBadgeDto
                {
                    UserBadgeId = ub.UserBadgeId,
                    BadgeId = ub.BadgeId,
                    UserId = ub.UserId,
                    AwardedDate = ub.AwardedDate,
                    Progress = ub.Progress,
                    IsAwarded = ub.IsAwarded,
                    TenantId = ub.TenantId,
                    BadgeName = ub.Badge.Name,
                    BadgeDescription = ub.Badge.Description,
                    //   BadgeType = ub.Badge.BadgeType,
                    UserName = "User Name", // You'll need to join with Users table
                    CreatedAt = ub.CreatedAt
                }).ToList();

                return new GetAllRecord<GetUserBadgeDto>
                {
                    StatusCode = StatusCode.Success,
                    StatusMessage = UserBadgeStatusMessage.UserBadgesFetched,
                    GetAllData = dtoList
                };
            }
            catch (Exception ex)
            {
                await _exceptionLogger.LogExceptionAsync(ex, "UserBadge-Module", "GetAllUserBadgesAsync", tenantId, null);
                return new GetAllRecord<GetUserBadgeDto>
                {
                    StatusCode = StatusCode.Error,
                    StatusMessage = $"{UserBadgeStatusMessage.UserBadgesFetchFailed}: {ex.Message}"
                };
            }
        }

        public async Task<GetSpecificRecord<GetUserBadgeDto>> GetUserBadgeByIdAsync(int userBadgeId, int tenantId)
        {
            try
            {
                using var tenantDb = _tenantDbContext.GetTenantDbContext(tenantId);

                var entity = await tenantDb.UserBadges
                    .Include(ub => ub.Badge)
                    .FirstOrDefaultAsync(ub => ub.UserBadgeId == userBadgeId && ub.TenantId == tenantId);

                if (entity == null)
                    return new GetSpecificRecord<GetUserBadgeDto>
                    {
                        StatusCode = StatusCode.NotFound,
                        StatusMessage = UserBadgeStatusMessage.UserBadgeNotFound
                    };

                var dto = new GetUserBadgeDto
                {
                    UserBadgeId = entity.UserBadgeId,
                    BadgeId = entity.BadgeId,
                    UserId = entity.UserId,
                    AwardedDate = entity.AwardedDate,
                    Progress = entity.Progress,
                    IsAwarded = entity.IsAwarded,
                    TenantId = entity.TenantId,
                    BadgeName = entity.Badge.Name,
                    BadgeDescription = entity.Badge.Description,
                    //   BadgeType = entity.Badge.BadgeType,
                    UserName = "User Name", // You'll need to join with Users table
                    CreatedAt = entity.CreatedAt
                };

                return new GetSpecificRecord<GetUserBadgeDto>
                {
                    StatusCode = StatusCode.Success,
                    StatusMessage = UserBadgeStatusMessage.UserBadgeFetched,
                    Data = dto
                };
            }
            catch (Exception ex)
            {
                await _exceptionLogger.LogExceptionAsync(ex, "UserBadge-Module", "GetUserBadgeByIdAsync", tenantId, null);
                return new GetSpecificRecord<GetUserBadgeDto>
                {
                    StatusCode = StatusCode.Error,
                    StatusMessage = $"{UserBadgeStatusMessage.UserBadgeFetchFailed}: {ex.Message}"
                };
            }
        }

        public async Task<GetAllRecord<GetUserBadgeDto>> GetUserBadgesByUserAsync(int userId, int tenantId)
        {
            try
            {
                using var tenantDb = _tenantDbContext.GetTenantDbContext(tenantId);

                var entities = await tenantDb.UserBadges
                    .Include(ub => ub.Badge)
                    .Where(ub => ub.UserId == userId && ub.TenantId == tenantId)
                    .OrderByDescending(ub => ub.CreatedAt)
                    .ToListAsync();

                var dtoList = entities.Select(ub => new GetUserBadgeDto
                {
                    UserBadgeId = ub.UserBadgeId,
                    BadgeId = ub.BadgeId,
                    UserId = ub.UserId,
                    AwardedDate = ub.AwardedDate,
                    Progress = ub.Progress,
                    IsAwarded = ub.IsAwarded,
                    TenantId = ub.TenantId,
                    BadgeName = ub.Badge.Name,
                    BadgeDescription = ub.Badge.Description,
                    //   BadgeType = ub.Badge.BadgeType,
                    UserName = "User Name", // You'll need to join with Users table
                    CreatedAt = ub.CreatedAt
                }).ToList();

                return new GetAllRecord<GetUserBadgeDto>
                {
                    StatusCode = StatusCode.Success,
                    StatusMessage = UserBadgeStatusMessage.UserBadgesFetched,
                    GetAllData = dtoList
                };
            }
            catch (Exception ex)
            {
                await _exceptionLogger.LogExceptionAsync(ex, "UserBadge-Module", "GetUserBadgesByUserAsync", tenantId, null);
                return new GetAllRecord<GetUserBadgeDto>
                {
                    StatusCode = StatusCode.Error,
                    StatusMessage = $"{UserBadgeStatusMessage.UserBadgesFetchFailed} : {ex.Message}"
                };
            }
        }

        public async Task<GetAllRecord<GetUserBadgeDto>> GetUserBadgesByBadgeAsync(int badgeId, int tenantId)
        {
            try
            {
                using var tenantDb = _tenantDbContext.GetTenantDbContext(tenantId);

                var entities = await tenantDb.UserBadges
                    .Include(ub => ub.Badge)
                    .Where(ub => ub.BadgeId == badgeId && ub.TenantId == tenantId)
                    .OrderByDescending(ub => ub.CreatedAt)
                    .ToListAsync();

                var dtoList = entities.Select(ub => new GetUserBadgeDto
                {
                    UserBadgeId = ub.UserBadgeId,
                    BadgeId = ub.BadgeId,
                    UserId = ub.UserId,
                    AwardedDate = ub.AwardedDate,
                    Progress = ub.Progress,
                    IsAwarded = ub.IsAwarded,
                    TenantId = ub.TenantId,
                    BadgeName = ub.Badge.Name,
                    BadgeDescription = ub.Badge.Description,
                    //    BadgeType = ub.Badge.BadgeType,
                    UserName = "User Name", // You'll need to join with Users table
                    CreatedAt = ub.CreatedAt
                }).ToList();

                return new GetAllRecord<GetUserBadgeDto>
                {
                    StatusCode = StatusCode.Success,
                    StatusMessage = UserBadgeStatusMessage.UserBadgesFetched,
                    GetAllData = dtoList
                };
            }
            catch (Exception ex)
            {
                await _exceptionLogger.LogExceptionAsync(ex, "UserBadge-Module", "GetUserBadgesByBadgeAsync", tenantId, null);
                return new GetAllRecord<GetUserBadgeDto>
                {
                    StatusCode = StatusCode.Error,
                    StatusMessage = $"{UserBadgeStatusMessage.UserBadgesFetchFailed} : {ex.Message}"
                };
            }
        }
    }
}