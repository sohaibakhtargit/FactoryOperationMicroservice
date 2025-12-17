using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Repositories.SuperAdmin.TeamManagement;
using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.ExceptionLogger;
using FactoryOps_AccessManagementService.FactoryOpsApp.Application.Common;
using FactoryOpsApp.Application.DTOs;
using FactoryOpsApp.Infrastructure.DBContext;
using Microsoft.EntityFrameworkCore;
using static FactoryOps_AccessManagementService.FactoryOpsApp.Common.CommonConstant;

namespace FactoryOperation_AccessManagementService.FactoryOpsApp.Infrastructure.Implementation.Repository.SuperAdmin.TeamManagement
{
    public class SuperAdminTeamRepository : ISuperAdminTeamRepository
    {
        private readonly MasterFactoryOpsDbContext _masterDbContext;
        private readonly TenantDbContextFactory _tenantDbContextFactory;
        private readonly IExceptionLoggerService _exceptionLogger;
        public SuperAdminTeamRepository(
            MasterFactoryOpsDbContext masterDbContext,
            TenantDbContextFactory tenantDbContextFactory,
            IExceptionLoggerService exceptionLogger)
        {
            _masterDbContext = masterDbContext;
            _tenantDbContextFactory = tenantDbContextFactory;
            _exceptionLogger = exceptionLogger;
        }

        public async Task<GetAllRecord<SuperAdminTeamDto>> GetAllTeamsAsync()
        {
            GetAllRecord<SuperAdminTeamDto> response = new();

            try
            {
                var tenants = await _masterDbContext.FactoryTenants
                    .Where(t => !t.IsDeleted && t.IsActive)
                    .ToListAsync();

                var tasks = tenants.Select(async tenant =>
                {
                    try
                    {
                        using var tenantDb = _tenantDbContextFactory.GetTenantDbContext(tenant.TenantId);

                        var teams = await tenantDb.FactoryTeams
                            .AsNoTracking()
                            .Include(t => t.Manager)
                            .Where(t => !t.IsDeleted && (t.Manager == null || !t.Manager.IsDeleted))

                            .Select(t => new SuperAdminTeamDto
                            {
                                TeamId = t.TeamId,
                                TeamName = t.Name,
                                TenantId = tenant.TenantId,
                                TenantName = tenant.TenantName
                            })
                            .ToListAsync();

                        return teams;
                    }
                    catch (Exception)
                    {
                        // Handle per-tenant failure gracefully
                        return new List<SuperAdminTeamDto>();
                    }
                });

                var allTeams = (await Task.WhenAll(tasks)).SelectMany(t => t).ToList();

                response.StatusCode = StatusCode.Success;
                response.StatusMessage = SuperAdminTeamStatusMessage.DataFetched;
                response.GetAllData = allTeams;
            }
            catch (Exception ex)
            {
                await _exceptionLogger.LogExceptionAsync(
                       ex,
                       sourceModule: "TeamModule",
                       apiName: "GetAllTeamsAsync",
                       tenantId: null,
                       userId: null
                   );
                response.StatusCode = StatusCode.Error;
                response.StatusMessage = SuperAdminTeamStatusMessage.FetchFailed;
            }

            return response;
        }

        public async Task<GetAllRecord<SuperAdminTeamDto>> GetAllTeamsFromAllTenantsAsync()
        {
            // Reuse the same logic as GetAllTeamsAsync()
            return await GetAllTeamsAsync();
        }
    }
}
