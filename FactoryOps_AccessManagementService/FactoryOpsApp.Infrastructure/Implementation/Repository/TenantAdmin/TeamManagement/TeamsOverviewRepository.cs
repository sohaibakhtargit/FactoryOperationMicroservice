using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Repositories.TenantAdmin.TeamManagement;
using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Services.SuperAdmin.AuditLogs;
using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.ExceptionLogger;
using FactoryOps_AccessManagementService.FactoryOpsApp.Application.Common;
using FactoryOpsApp.Application.DTOs;
using FactoryOpsApp.Infrastructure.DBContext;

namespace FactoryOperation_AccessManagementService.FactoryOpsApp.Infrastructure.Implementation.Repository.TenantAdmin.TeamManagement
{
    public class TeamsOverviewRepository : ITeamsOverviewRepository
    {
        private readonly TenantDbContextFactory _tenantDbContext;
        private readonly MasterFactoryOpsDbContext _masterDbcontext;
        private readonly IAuditLogService _auditLogger;
        private readonly IExceptionLoggerService _exceptionLogger;
        public TeamsOverviewRepository(TenantDbContextFactory tenantDbContext,
                             IAuditLogService auditLogger,
                             MasterFactoryOpsDbContext masterDbcontext,
                             IExceptionLoggerService exceptionLogger)
        {
            _tenantDbContext = tenantDbContext;
            _masterDbcontext = masterDbcontext;
            _auditLogger = auditLogger;
            _exceptionLogger = exceptionLogger;
        }

        public async Task<GetAllRecord<TeamOverviewDto>> GetTeamOverviewAsync(int tenantId)
        {
            GetAllRecord<TeamOverviewDto> response = new();
            try
            {
                var tenant = _masterDbcontext.FactoryTenants
                                            .FirstOrDefault(l => l.TenantId == tenantId && !l.IsDeleted);
                if (tenant == null)
                {
                    response.StatusCode = "404";
                    response.StatusMessage = "Tenant not found";
                }
                using var tenantDb = _tenantDbContext.GetTenantDbContext(tenantId);
                var allMembers = (from member in tenantDb.FactoryTeamMembers
                                  where !member.IsDeleted
                                  join user in tenantDb.FactoryUsers
                                      on member.UserId equals user.UserId into userJoin
                                  from u in userJoin.DefaultIfEmpty()
                                  select new
                                  {
                                      member.TeamId,
                                      member.UserId,
                                      MemberName = u != null ? u.FirstName + " " + u.LastName : string.Empty
                                  }).ToList();
                var allUsers = tenantDb.FactoryUsers
                               .Select(u => new
                               {
                                   u.TenantId,
                                   u.UserId,
                                   u.Username
                               })
                               .ToList();
                var allLocations = tenantDb.Locations
                                            .Where(l => l.TenantId == tenantId && !l.IsDeleted)
                                            .Select(l => new
                                            {
                                                l.TenantId,
                                                l.LocationId,
                                                l.LocationName
                                            })
                                            .ToList();
                var allAssignments = tenantDb.PointAssignments
                                             .Where(p => p.IsActive && p.TenantId == tenantId && p.TeamId != 0)
                                             .Select(p => new
                                             {
                                                 p.TenantId,
                                                 p.TaskName,
                                                 p.AssignedToUserId,
                                                 p.TotalPoints,
                                                 p.TeamId
                                             })
                                             .ToList();
                var teamsOverview = tenantDb.FactoryTeams
                                    .Where(t => !t.IsDeleted)
                                    .AsEnumerable()
                                    .Select(t =>
                                    {
                                        var teamAssignments = allAssignments.Where(a => a.TeamId == t.TeamId
                                                                            && a.TenantId == t.TenantId && t.TeamId != 0).ToList();
                                        var location = allLocations.FirstOrDefault(l => l.LocationId == (t.Site ?? 0) && l.TenantId == t.TenantId);
                                        return teamAssignments.Any()
                                            ? teamAssignments.Select(a => new TeamOverviewDto
                                            {
                                                TenantId = tenantId,
                                                TeamId = t.TeamId,
                                                TeamName = t.Name,
                                                //TaskId = a.TaskId,
                                                TaskName = a.TaskName,
                                                AssignedtoUserId = a.AssignedToUserId,
                                                UserName = allUsers.FirstOrDefault(u => u.UserId == a.AssignedToUserId && u.TenantId == a.TenantId)?.Username ?? string.Empty,
                                                TotalPoints = a.TotalPoints,
                                                SiteId = location?.LocationId ?? 0,
                                                SiteName = location?.LocationName ?? string.Empty,
                                                //DepartmentId = t.DepartmentId ?? 0,
                                                Department = t.Department ?? string.Empty,
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
                                                    })
                                                    .ToList()
                                            }).ToList()
                                            : new List<TeamOverviewDto>
                                            {
                                        new TeamOverviewDto
                                        {
                                            TenantId = tenantId,
                                            TeamId = t.TeamId,
                                            TeamName = t.Name,
                                            //TaskId = 0,
                                            TaskName = string.Empty,
                                            AssignedtoUserId = 0,
                                            UserName = string.Empty,
                                            TotalPoints = 0,
                                            SiteId = location?.LocationId ?? 0,
                                            SiteName = location?.LocationName ?? string.Empty,
                                           // DepartmentId = t.DepartmentId ?? 0,
                                            Department = t.Department ?? string.Empty,
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
                                                })
                                                .ToList()
                                        }
                                            };
                                    })
                                    .SelectMany(x => x)
                                    .ToList();
                response.StatusCode = "200";
                response.StatusMessage = "Success";
                response.GetAllData = teamsOverview;
            }
            catch (Exception ex)
            {
                _exceptionLogger.LogExceptionAsync(
                    ex,
                    sourceModule: "TeamModule",
                    apiName: "GetTeamOverview",
                    tenantId: tenantId,
                    userId: null
                );
                response.StatusCode = "500";
                response.StatusMessage = $"Error fetching team overview: {ex.Message}";
            }
            return response;
        }
    }
}