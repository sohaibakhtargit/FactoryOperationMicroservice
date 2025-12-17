using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Repositories.TenantAdmin.TeamManagement;
using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Services.SuperAdmin.AuditLogs;
using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.ExceptionLogger;
using FactoryOps_AccessManagementService.FactoryOpsApp.Application.Common;
using FactoryOpsApp.Application.DTOs;
using FactoryOpsApp.Infrastructure.DBContext;
using Microsoft.EntityFrameworkCore;
using static FactoryOps_AccessManagementService.FactoryOpsApp.Common.CommonConstant;

namespace FactoryOperation_AccessManagementService.FactoryOpsApp.Infrastructure.Implementation.Repository.TenantAdmin.TeamManagement
{
    public class BadgeRepository : IBadgeRepository
    {
        private readonly TenantDbContextFactory _tenantDbContext;
        private readonly IAuditLogService _auditLogger;
        private readonly IExceptionLoggerService _exceptionLogger;

        public BadgeRepository(TenantDbContextFactory tenantDbContext,
                             IAuditLogService auditLogger,
                             IExceptionLoggerService exceptionLogger)
        {
            _tenantDbContext = tenantDbContext;
            _auditLogger = auditLogger;
            _exceptionLogger = exceptionLogger;
        }

        public async Task<CommonResponseModel> AddBadgeAsync(BadgeDto dto)
        {
            try
            {
                using var tenantDb = _tenantDbContext.GetTenantDbContext(dto.TenantId);

                var entity = new Badge
                {
                    Name = dto.Name,
                    Description = dto.Description,
                    BadgeType = dto.BadgeType,
                    PointsRequired = dto.PointsRequired,
                    TasksRequired = dto.TasksRequired,
                    DaysRequired = dto.DaysRequired,
                    TeamId = dto.TeamId,
                    TenantId = dto.TenantId,
                    IsActive = dto.IsActive,
                    CreatedAt = DateTime.UtcNow,
                    Rarity = dto.Rarity,
                };

                await tenantDb.Badges.AddAsync(entity);
                await tenantDb.SaveChangesAsync();

                await _auditLogger.LogAuditAsync("Create", $"Created Badge '{entity.Name}'", dto.TenantId, "", "AddBadgeAsync");

                return new CommonResponseModel { StatusCode = StatusCode.Success, StatusMessage = BadgeStatusMessage.BadgeCreated };
            }
            catch (Exception ex)
            {
                await _exceptionLogger.LogExceptionAsync(ex, "Badge-Module", "AddBadgeAsync", dto.TenantId, null);
                return new CommonResponseModel { StatusCode = StatusCode.Error, StatusMessage = $"Error: {ex.Message}" };
            }
        }

        public async Task<CommonResponseModel> UpdateBadgeAsync(BadgeDto dto)
        {
            try
            {
                using var tenantDb = _tenantDbContext.GetTenantDbContext(dto.TenantId);

                var entity = await tenantDb.Badges
                    .FirstOrDefaultAsync(b => b.BadgeId == dto.BadgeId && b.TenantId == dto.TenantId);

                if (entity == null)
                    return new CommonResponseModel { StatusCode = StatusCode.NotFound, StatusMessage = BadgeStatusMessage.BadgeNotFound };

                entity.Name = dto.Name;
                entity.Description = dto.Description;
                entity.BadgeType = dto.BadgeType;
                entity.PointsRequired = dto.PointsRequired;
                entity.TasksRequired = dto.TasksRequired;
                entity.DaysRequired = dto.DaysRequired;
                entity.TeamId = dto.TeamId;
                entity.IsActive = dto.IsActive;
                entity.Rarity = dto.Rarity;
                await tenantDb.SaveChangesAsync();

                await _auditLogger.LogAuditAsync("Update", $"Updated Badge '{entity.Name}'", dto.TenantId, "", "UpdateBadgeAsync");

                return new CommonResponseModel { StatusCode = StatusCode.Success, StatusMessage = BadgeStatusMessage.BadgeUpdated };
            }
            catch (Exception ex)
            {
                await _exceptionLogger.LogExceptionAsync(ex, "Badge-Module", "UpdateBadgeAsync", dto.TenantId, null);
                return new CommonResponseModel { StatusCode = StatusCode.Error, StatusMessage = $"Error: {ex.Message}" };
            }
        }

        public async Task<CommonResponseModel> DeleteBadgeAsync(int badgeId, int tenantId)
        {
            try
            {
                using var tenantDb = _tenantDbContext.GetTenantDbContext(tenantId);

                var entity = await tenantDb.Badges
                    .FirstOrDefaultAsync(b => b.BadgeId == badgeId && b.TenantId == tenantId);

                if (entity == null)
                    return new CommonResponseModel { StatusCode = StatusCode.NotFound, StatusMessage = BadgeStatusMessage.BadgeNotFound };

                entity.IsActive = false;

                await tenantDb.SaveChangesAsync();

                await _auditLogger.LogAuditAsync("Delete", $"Deleted Badge '{entity.Name}'", tenantId, "", "DeleteBadgeAsync");

                return new CommonResponseModel { StatusCode = StatusCode.Success, StatusMessage = BadgeStatusMessage.BadgeDeleted };
            }
            catch (Exception ex)
            {
                await _exceptionLogger.LogExceptionAsync(ex, "Badge-Module", "DeleteBadgeAsync", tenantId, null);
                return new CommonResponseModel { StatusCode = StatusCode.Error, StatusMessage = $"Error: {ex.Message}" };
            }
        }

        public async Task<GetAllRecord<GetBadgeDto>> GetAllBadgesAsync(int tenantId)
        {
            try
            {
                using var tenantDb = _tenantDbContext.GetTenantDbContext(tenantId);
                var entities = await tenantDb.Badges
                    .Include(b => b.UserBadges)
                    .Include(b => b.FactoryTeam)
                    .ToListAsync();

                var dtoList = new List<GetBadgeDto>();

                foreach (var b in entities
                    .Where(b => b.TenantId == tenantId && b.IsActive)
                    .OrderBy(b => b.Name))
                {
                    var teamMembers = new List<TeamMemberDto>();
                    if (b.TeamId.HasValue)
                    {
                        teamMembers = await tenantDb.FactoryTeamMembers
                            .Include(m => m.User)
                            .Where(m => m.TeamId == b.TeamId.Value && m.IsActive && !m.IsDeleted)
                            .Select(m => new TeamMemberDto
                            {
                                UserId = m.UserId,
                                UserName = m.User != null
                                    ? $"{m.User.FirstName} {(string.IsNullOrEmpty(m.User.LastName) ? "" : m.User.LastName)}".Trim()
                                    : null
                            })
                            .ToListAsync();
                    }

                    dtoList.Add(new GetBadgeDto
                    {
                        BadgeId = b.BadgeId,
                        Name = b.Name,
                        Description = b.Description,
                        BadgeType = b.BadgeType,
                        PointsRequired = b.PointsRequired,
                        TasksRequired = b.TasksRequired,
                        DaysRequired = b.DaysRequired,
                        TenantId = b.TenantId,
                        IsActive = b.IsActive,
                        CreatedAt = b.CreatedAt,
                        TotalAwarded = b.UserBadges?.Count(ub => ub.IsAwarded) ?? 0,
                        TeamId = b.TeamId,
                        TeamName = b.FactoryTeam?.Name,
                        TeamMembers = teamMembers,
                        Rarity = b.Rarity,

                    });
                }

                return new GetAllRecord<GetBadgeDto>
                {
                    StatusCode = StatusCode.Success,
                    StatusMessage = BadgeStatusMessage.BadgesFetched,
                    GetAllData = dtoList
                };
            }
            catch (Exception ex)
            {
                await _exceptionLogger.LogExceptionAsync(ex, "Badge-Module", "GetAllBadgesAsync", tenantId, null);
                return new GetAllRecord<GetBadgeDto>
                {
                    StatusCode = StatusCode.Error,
                    StatusMessage = $"Error: {ex.Message}"
                };
            }
        }
        public async Task<GetSpecificRecord<GetBadgeDto>> GetBadgeByIdAsync(int badgeId, int tenantId)
        {
            try
            {
                using var tenantDb = _tenantDbContext.GetTenantDbContext(tenantId);

                var entity = await tenantDb.Badges
                    .Include(b => b.UserBadges)
                    .Include(b => b.FactoryTeam)
                    .FirstOrDefaultAsync(b => b.BadgeId == badgeId && b.TenantId == tenantId && b.IsActive);

                if (entity == null)
                    return new GetSpecificRecord<GetBadgeDto>
                    {
                        StatusCode = StatusCode.NotFound,
                        StatusMessage = BadgeStatusMessage.BadgeNotFound,
                    };
                var teamMembers = await tenantDb.FactoryTeamMembers
                    .Where(m => m.TeamId == entity.TeamId && m.IsActive && !m.IsDeleted)
                    .Select(m => new TeamMemberDto
                    {
                        UserId = m.UserId,
                        UserName = m.User != null
                            ? $"{m.User.FirstName} {(string.IsNullOrEmpty(m.User.LastName) ? "" : m.User.LastName)}".Trim()
                            : null
                    })
                    .ToListAsync();

                var dto = new GetBadgeDto
                {
                    BadgeId = entity.BadgeId,
                    Name = entity.Name,
                    Description = entity.Description,
                    BadgeType = entity.BadgeType,
                    PointsRequired = entity.PointsRequired,
                    TasksRequired = entity.TasksRequired,
                    DaysRequired = entity.DaysRequired,
                    TenantId = entity.TenantId,
                    IsActive = entity.IsActive,
                    CreatedAt = entity.CreatedAt,
                    TotalAwarded = entity.UserBadges?.Count(ub => ub.IsAwarded) ?? 0,
                    TeamId = entity.TeamId,
                    TeamName = entity.FactoryTeam?.Name,
                    TeamMembers = teamMembers,
                    Rarity = entity.Rarity
                };

                return new GetSpecificRecord<GetBadgeDto>
                {
                    StatusCode = StatusCode.Success,
                    StatusMessage = BadgeStatusMessage.Success,
                    Data = dto
                };
            }
            catch (Exception ex)
            {
                await _exceptionLogger.LogExceptionAsync(ex, "Badge-Module", "GetBadgeByIdAsync", tenantId, null);
                return new GetSpecificRecord<GetBadgeDto>
                {
                    StatusCode = StatusCode.Error,
                    StatusMessage = $"Error: {ex.Message}"
                };
            }
        }
    }
}