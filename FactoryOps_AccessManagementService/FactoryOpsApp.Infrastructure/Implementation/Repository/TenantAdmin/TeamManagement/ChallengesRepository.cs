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
    public class ChallengesRepository : IChallengesRepository
    {
        private readonly TenantDbContextFactory _tenantDbContext;
        private readonly IExceptionLoggerService _exceptionLogger;
        private readonly IAuditLogService _auditLogger;

        public ChallengesRepository(
            TenantDbContextFactory tenantDbContext,
            IExceptionLoggerService exceptionLogger,
            IAuditLogService auditLogger)
        {
            _tenantDbContext = tenantDbContext;
            _exceptionLogger = exceptionLogger;
            _auditLogger = auditLogger;
        }
        public async Task<CommonResponseModel> CreateChallengesAsync(ChallengesDto dto)
        {
            var response = new CommonResponseModel();

            try
            {
                using var tenantDb = _tenantDbContext.GetTenantDbContext(dto.TenantId);
                if (dto.TeamId.HasValue)
                {
                    var teamExists = await tenantDb.FactoryTeams
                        .AnyAsync(t => t.TeamId == dto.TeamId.Value);

                    if (!teamExists)
                        dto.TeamId = null;
                }

                var entity = new Challenge
                {
                    TenantId = dto.TenantId,
                    Title = dto.Title,
                    Description = dto.Description,
                    ChallengeType = dto.ChallengeType,
                    Reward = dto.Reward,
                    Requirements = dto.Requirements,
                    //ParticipantIds = dto.ParticipantIds,
                    ProgressData = dto.ProgressData,
                    MaxParticipants = dto.MaxParticipants,
                    TeamId = dto.TeamId,
                    //Status = dto.Status,
                    StartDate = dto.StartDate,
                    EndDate = dto.EndDate,
                    IsActive = true,
                    CreatedAt = dto.CreatedAt,

                };

                tenantDb.Challenges.Add(entity);
                await tenantDb.SaveChangesAsync();

                response.StatusCode = StatusCode.Success;
                response.StatusMessage = ChallengeStatusMessage.ChallengeAdded;
            }
            catch (Exception ex)
            {
                await _exceptionLogger.LogExceptionAsync(ex, "Challenge-Module", "ChallengeSubTask", dto.TenantId, null);
                response.StatusCode = StatusCode.Error;
                response.StatusMessage = $"{ChallengeStatusMessage.ChallengeAddFailed}: {ex.Message}";
            }

            return response;
        }
        public async Task<GetAllRecord<ChallengeBoardData>> GetChallengeBoardAsync(int tenantId)
        {
            var response = new GetAllRecord<ChallengeBoardData>();

            try
            {
                using var tenantDb = _tenantDbContext.GetTenantDbContext(tenantId);
                var now = DateTime.UtcNow;

                var rows = await (
                    from c in tenantDb.Challenges.AsNoTracking()
                    where c.TenantId == tenantId
                    join wo in tenantDb.WorkOrders.AsNoTracking() on c.TeamId equals wo.AssignedToTeamId into woJoin
                    from wo in woJoin.DefaultIfEmpty()
                    join t in tenantDb.FactoryTeams.AsNoTracking() on wo.AssignedToTeamId equals t.TeamId into teamJoin
                    from t in teamJoin.DefaultIfEmpty()
                    select new
                    {
                        c.ChallengeId,
                        c.TenantId,
                        c.Title,
                        c.Description,
                        c.ChallengeType,
                        c.Reward,
                        c.Requirements,
                        // c.ParticipantIds,
                        c.StartDate,
                        c.EndDate,
                        c.IsActive,
                        c.MaxParticipants,
                        //c.Status,
                        TeamName = t != null ? t.Name : null
                    }
                ).ToListAsync();

                var allCards = rows.Select(r => new GetChallengesCardDetails
                {
                    ChallengeId = r.ChallengeId,
                    TenantId = r.TenantId,
                    Title = r.Title,
                    Description = r.Description,
                    ChallengeType = r.ChallengeType.ToString(),
                    IsActive = r.IsActive,
                    StartDate = r.StartDate,
                    EndDate = r.EndDate,
                    DaysLeft = (int)Math.Ceiling((r.EndDate - now).TotalDays),
                    //  ParticipantsCount = r.ParticipantIds?.Count ?? 0,
                    MaxParticipants = r.MaxParticipants,
                    Reward = r.Reward,
                    TeamName = r.TeamName
                }).ToList();

                var upcoming = allCards.Where(c => c.StartDate > now).ToList();
                var ongoing = allCards.Where(c => c.StartDate <= now && c.EndDate >= now).ToList();
                var completed = allCards.Where(c => c.EndDate < now).ToList();

                response.StatusCode = StatusCode.Success;
                response.StatusMessage = ChallengeStatusMessage.ChallengeBoardFetched;
                response.GetAllData = new List<ChallengeBoardData>
                {
                    new ChallengeBoardData
                    {
                        UpcomingChallenges = upcoming,
                        OngoingChallenges = ongoing,
                        CompletedChallenges = completed
                    }
                };
            }
            catch (Exception ex)
            {
                await _exceptionLogger.LogExceptionAsync(ex, "Challenge-Module", "GetChallengeBoard", tenantId, null);
                response.StatusCode = StatusCode.Error;
                response.StatusMessage = $"{ChallengeStatusMessage.ChallengeBoardFetchFailed}: {ex.Message}";
            }

            return response;
        }


    }
}
