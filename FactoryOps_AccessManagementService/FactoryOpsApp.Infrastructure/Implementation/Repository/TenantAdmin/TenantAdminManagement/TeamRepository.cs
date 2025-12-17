using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Repositories.TenantAdmin.TenantAdminManagement;
using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Services.SuperAdmin.AuditLogs;
using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.ExceptionLogger;
using FactoryOps_AccessManagementService.FactoryOpsApp.Application.Common;
using FactoryOpsApp.Application.DTOs;
using FactoryOpsApp.Domain.Entities.FactoryOpsTenants;
using FactoryOpsApp.Domain.Entities.MasterTenantsAdmin;
using FactoryOpsApp.Infrastructure.DBContext;
using Microsoft.EntityFrameworkCore;
using static FactoryOps_AccessManagementService.FactoryOpsApp.Common.CommonConstant;

namespace FactoryOperation_AccessManagementService.FactoryOpsApp.Infrastructure.Implementation.Repository.TenantAdmin.TenantAdminManagement
{
    public class TeamRepository : ITeamRepository
    {
        private readonly TenantDbContextFactory _tenantDbContext;
        private readonly MasterFactoryOpsDbContext _masterDbcontext;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IExceptionLoggerService _exceptionLogger;
        private readonly IAuditLogService _auditLogger;
        public TeamRepository(TenantDbContextFactory tenantDbContext,
            MasterFactoryOpsDbContext masterDbcontext,
            IHttpContextAccessor httpContextAccessor,
            IExceptionLoggerService exceptionLogger,
            IAuditLogService auditLogger
            )
        {
            _tenantDbContext = tenantDbContext;
            _masterDbcontext = masterDbcontext;
            _httpContextAccessor = httpContextAccessor;
            _exceptionLogger = exceptionLogger;
            _auditLogger = auditLogger;
        }

        public async Task<CommonResponseModel> AddTeam(AddTeamDto dto)
        {
            CommonResponseModel response = new();
            using var tenantDb = _tenantDbContext.GetTenantDbContext(dto.TenantId);
            try
            {
                var exists = await tenantDb.FactoryTeams
                    .AnyAsync(t => t.Name.ToLower() == dto.Name.ToLower() && !t.IsDeleted);

                if (exists)
                {
                    response.StatusCode = StatusCode.BadRequest;
                    response.StatusMessage = TeamStatusMessage.TeamAlreadyExists;
                    return response;
                }

                var team = new FactoryTeam
                {
                    TenantId = dto.TenantId,
                    Name = dto.Name,
                    Description = dto.Description,
                    ManagerId = dto.ManagerId ?? null,
                    Site = dto.Site ?? null,
                    Department = dto.Department ?? " ",
                    IsActive = true,
                    IsDeleted = false,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = dto.TenantId
                };

                await tenantDb.FactoryTeams.AddAsync(team);
                await tenantDb.SaveChangesAsync();

                if (dto.MemberIds != null && dto.MemberIds.Any())
                {
                    var members = dto.MemberIds
                        .Where(uid => uid != dto.ManagerId)
                        .Select(uid => new FactoryTeamMembers
                        {
                            TeamId = team.TeamId,
                            UserId = uid,
                            IsActive = true,
                            IsDeleted = false,
                            CreatedAt = DateTime.UtcNow,
                            CreatedBy = dto.TenantId
                        }).ToList();

                    await tenantDb.FactoryTeamMembers.AddRangeAsync(members);
                    await tenantDb.SaveChangesAsync();
                }

                var ctx = _httpContextAccessor.HttpContext;
                var auditData = new Audit_Log_MasterDb()
                {
                    Action = "Create",
                    Details = $"Add-Team ({dto.Name})",
                    EventType = "TeamCreated",
                    TenantId = dto.TenantId,
                    Email = "",
                    Timestamp = DateTime.UtcNow,
                    IsActive = true,
                    IsDeleted = false,
                    UserName = Environment.UserName,
                    Ipaddress = ctx?.Connection.RemoteIpAddress?.ToString(),
                };

                await _masterDbcontext.Audit_Log_MasterDb.AddAsync(auditData);
                await _masterDbcontext.SaveChangesAsync();

                response.StatusCode = StatusCode.Success;
                response.StatusMessage = TeamStatusMessage.TeamAdded;
            }
            catch (Exception ex)
            {
                await _exceptionLogger.LogExceptionAsync(
                    ex,
                    sourceModule: "TeamModule",
                    apiName: "AddTeam",
                    tenantId: dto?.TenantId,
                    userId: null
                );
                response.StatusCode = StatusCode.Error;
                response.StatusMessage = $"{TeamStatusMessage.TeamAddFailed}: {ex.Message}";
            }

            return response;
        }
        public async Task<CommonResponseModel> UpdateTeam(int id, AddTeamDto dto)
        {
            CommonResponseModel response = new();

            using var transaction = await _masterDbcontext.Database.BeginTransactionAsync();
            try
            {
                using var tenantDb = _tenantDbContext.GetTenantDbContext(dto.TenantId);

                var team = await tenantDb.FactoryTeams
                    .FirstOrDefaultAsync(t => t.TeamId == id && !t.IsDeleted);

                if (team == null)
                {
                    response.StatusCode = StatusCode.NotFound;
                    response.StatusMessage = TeamStatusMessage.TeamNotFound;
                    return response;
                }

                team.Name = dto.Name;
                team.Description = dto.Description;
                team.ManagerId = dto.ManagerId;
                team.Site = dto.Site;
                team.Department = dto.Department;
                team.UpdatedAt = DateTime.UtcNow;
                team.UpdatedBy = dto.TenantId;

                var existingMappings = await tenantDb.FactoryTeamMembers
                    .Where(m => m.TeamId == id)
                    .ToListAsync();

                var requestedMemberIds = dto.MemberIds?.Where(uid => uid != dto.ManagerId).ToList() ?? new List<int?>();

                foreach (var mapping in existingMappings)
                {
                    if (!requestedMemberIds.Contains(mapping.UserId))
                    {
                        if (!mapping.IsDeleted)
                        {
                            mapping.IsDeleted = true;
                            mapping.IsActive = false;
                        }
                    }
                    else
                    {
                        if (mapping.IsDeleted)
                        {
                            mapping.IsDeleted = false;
                            mapping.IsActive = true;
                        }
                    }
                }

                var existingUserIds = existingMappings.Select(m => m.UserId).ToList();
                foreach (var userId in requestedMemberIds)
                {
                    if (!existingUserIds.Contains(userId))
                    {
                        tenantDb.FactoryTeamMembers.Add(new FactoryTeamMembers
                        {
                            TeamId = id,
                            UserId = userId,
                            IsActive = true,
                            IsDeleted = false,
                            CreatedAt = DateTime.UtcNow,
                            CreatedBy = dto.TenantId
                        });
                    }
                }

                var ctx = _httpContextAccessor.HttpContext;
                var auditData = new Audit_Log_MasterDb()
                {
                    Action = "Update",
                    Details = $"Updated team '{team.Name}' (ID: {id}) with {requestedMemberIds.Count} members",
                    EventType = "TeamUpdate",
                    TenantId = dto.TenantId,
                    Email = ctx?.User?.Identity?.Name ?? "System",
                    Timestamp = DateTime.UtcNow,
                    IsActive = true,
                    IsDeleted = false,
                    UserName = Environment.UserName,
                    Ipaddress = ctx?.Connection?.RemoteIpAddress?.ToString(),
                };

                await _masterDbcontext.Audit_Log_MasterDb.AddAsync(auditData);

                await tenantDb.SaveChangesAsync();
                await _masterDbcontext.SaveChangesAsync();
                await transaction.CommitAsync();

                response.StatusCode = StatusCode.Success;
                response.StatusMessage = TeamStatusMessage.TeamUpdated;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                await _exceptionLogger.LogExceptionAsync(
                        ex,
                        sourceModule: "TeamModule",
                        apiName: "UpdateTeam",
                        tenantId: dto?.TenantId,
                        userId: null
                    );

                response.StatusCode = StatusCode.Success;
                response.StatusMessage = $"{TeamStatusMessage.TeamUpdateFailed}: {ex.Message}";
            }

            return response;
        }
        public async Task<CommonResponseModel> DeleteTeam(int id, int TenantId)
        {
            CommonResponseModel response = new();

            using var transaction = await _masterDbcontext.Database.BeginTransactionAsync();
            try
            {
                using var tenantDb = _tenantDbContext.GetTenantDbContext(TenantId);

                var team = await tenantDb.FactoryTeams
                    .FirstOrDefaultAsync(t => t.TeamId == id && !t.IsDeleted);

                if (team == null)
                {
                    response.StatusCode = StatusCode.NotFound;
                    response.StatusMessage = TeamStatusMessage.TeamNotFound;
                    return response;
                }

                team.IsDeleted = true;
                team.IsActive = false;
                team.DeletedAt = DateTime.UtcNow;
                team.DeletedBy = TenantId;

                var teamMembers = await tenantDb.FactoryTeamMembers
                    .Where(m => m.TeamId == id && !m.IsDeleted)
                    .ToListAsync();

                foreach (var member in teamMembers)
                {
                    member.IsDeleted = true;
                    member.IsActive = false;
                }

                var ctx = _httpContextAccessor.HttpContext;
                var auditData = new Audit_Log_MasterDb()
                {
                    Action = "Delete",
                    Details = $"Deleted team '{team.Name}' (ID: {id})",
                    EventType = "TeamDelete",
                    TenantId = TenantId,
                    Email = ctx?.User?.Identity?.Name ?? "System",
                    Timestamp = DateTime.UtcNow,
                    IsActive = true,
                    IsDeleted = false,
                    UserName = Environment.UserName,
                    Ipaddress = ctx?.Connection?.RemoteIpAddress?.ToString(),
                };

                await _masterDbcontext.Audit_Log_MasterDb.AddAsync(auditData);

                await tenantDb.SaveChangesAsync();
                await _masterDbcontext.SaveChangesAsync();
                await transaction.CommitAsync();

                response.StatusCode = StatusCode.Success;
                response.StatusMessage = TeamStatusMessage.TeamDeleted;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                await _exceptionLogger.LogExceptionAsync(
                        ex,
                        sourceModule: "TeamModule",
                        apiName: "DeleteTeam",
                        tenantId: TenantId,
                        userId: null
                    );
                response.StatusCode = StatusCode.Error;
                response.StatusMessage = $"{TeamStatusMessage.TeamDeleteFailed}: {ex.Message}";
            }

            return response;
        }
        public GetAllRecord<GetTeamDto> GetAllTeams(int TenantId)
        {
            GetAllRecord<GetTeamDto> response = new();

            try
            {
                var Tenant = _masterDbcontext.FactoryTenants.
                    FirstOrDefault(l => l.TenantId == TenantId && l.IsDeleted == false);

                if (Tenant == null)
                {
                    response.StatusCode = StatusCode.NotFound;
                    response.StatusMessage = TeamStatusMessage.TenantNotFound;
                    return response;
                }

                using var tenantDb = _tenantDbContext.GetTenantDbContext(TenantId);

                var allMembers = tenantDb.FactoryTeamMembers
                    .Include(m => m.User)
                    .Where(m => !m.IsDeleted)
                    .Select(m => new
                    {
                        m.TeamId,
                        m.UserId,
                        MemberName = m.User.FirstName + " " + m.User.LastName
                    })
                    .ToList();
                var teams = tenantDb.FactoryTeams
                    .Include(t => t.Manager).Include(t => t.Location)
                    .Where(t => !t.IsDeleted && (t.Manager == null || !t.Manager.IsDeleted))
                    .AsEnumerable()
                    .Select(t => new GetTeamDto
                    {
                        TeamId = t.TeamId,
                        TeamName = t.Name,
                        Description = t.Description,
                        TenantId = t.TenantId,
                        TenantName = Tenant.TenantName,
                        SiteId = t.Site ?? null,
                        Site = t.Location != null ? t.Location.LocationName : null,
                        Department = t.Department,
                        ManagerId = t.ManagerId ?? null,
                        ManagerName = t.Manager != null ? t.Manager.FirstName + " " + t.Manager.LastName : "",
                        IsActive = t.IsActive,
                        IsDeleted = t.IsDeleted,
                        CreatedAt = t.CreatedAt,
                        CreatedBy = t.CreatedBy,
                        UpdatedAt = t.UpdatedAt,
                        UpdatedBy = t.UpdatedBy,

                        Members = allMembers
                            .Where(m => m.TeamId == t.TeamId)
                            .Select(m => new TeamMemberMapDto
                            {
                                UserId = m.UserId,
                                MemberName = m.MemberName
                            }).ToList()
                    })
                    .ToList();

                response.StatusCode = StatusCode.Success;
                response.StatusMessage = TeamStatusMessage.TeamFetched;
                response.GetAllData = teams;
            }
            catch (Exception ex)
            {
                _exceptionLogger.LogExceptionAsync(
                   ex,
                   sourceModule: "TeamModule",
                   apiName: "GetAllTeams",
                   tenantId: TenantId,
                   userId: null
               );
                response.StatusCode = StatusCode.Error;
                response.StatusMessage = $"{TeamStatusMessage.TeamFetchFailed}: {ex.Message}";
            }

            return response;
        }
        public GetSpecificRecord<GetTeamDto> GetTeamsByIdAsync(int tenantId, int teamId)
        {
            var response = new GetSpecificRecord<GetTeamDto>();
            try
            {
                var Tenant = _masterDbcontext.FactoryTenants.
                    FirstOrDefault(l => l.TenantId == tenantId && l.IsDeleted == false);

                if (Tenant == null)
                {
                    response.StatusCode = StatusCode.NotFound;
                    response.StatusMessage = TeamStatusMessage.TenantNotFound;
                    return response;
                }

                using var tenantDb = _tenantDbContext.GetTenantDbContext(tenantId);

                var allMembers = tenantDb.FactoryTeamMembers
                    .Include(m => m.User)
                    .Where(m => !m.IsDeleted && m.TeamId == teamId)
                    .Select(m => new
                    {
                        m.TeamId,
                        m.UserId,
                        MemberName = m.User.FirstName + " " + m.User.LastName
                    })
                    .ToList();
                var teams = tenantDb.FactoryTeams
                    .Include(t => t.Manager).Include(t => t.Location)
                    .Where(t => !t.IsDeleted && (t.Manager == null || !t.Manager.IsDeleted) && t.TeamId == teamId)
                    .AsEnumerable()
                    .Select(t => new GetTeamDto
                    {
                        TeamId = t.TeamId,
                        TeamName = t.Name,
                        Description = t.Description,
                        TenantId = t.TenantId,
                        TenantName = Tenant.TenantName,
                        SiteId = t.Site ?? null,
                        Site = t.Location != null ? t.Location.LocationName : null,
                        Department = t.Department,
                        ManagerId = t.ManagerId ?? null,
                        ManagerName = t.Manager != null ? t.Manager.FirstName + " " + t.Manager.LastName : "",
                        IsActive = t.IsActive,
                        IsDeleted = t.IsDeleted,
                        CreatedAt = t.CreatedAt,
                        CreatedBy = t.CreatedBy,
                        UpdatedAt = t.UpdatedAt,
                        UpdatedBy = t.UpdatedBy,

                        Members = allMembers
                            .Where(m => m.TeamId == t.TeamId)
                            .Select(m => new TeamMemberMapDto
                            {
                                UserId = m.UserId,
                                MemberName = m.MemberName
                            }).ToList()
                    })
                    .FirstOrDefault();

                response.StatusCode = StatusCode.Success;
                response.StatusMessage = TeamStatusMessage.TeamFetched;
                response.Data = teams;
            }
            catch (Exception ex)
            {
                _exceptionLogger.LogExceptionAsync(
                   ex,
                   sourceModule: "TeamModule",
                   apiName: "GetTeamById",
                   tenantId: tenantId,
                   userId: null
               );
                response.StatusCode = StatusCode.Error;
                response.StatusMessage = $"{TeamStatusMessage.TeamFetchFailed}: {ex.Message}";
            }
            return response;
        }

    }
}
