using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Repositories.TenantAdmin.TenantAdminDashboard;
using FactoryOps_AccessManagementService.FactoryOpsApp.Application.Common;
using FactoryOpsApp.Infrastructure.DBContext;
using static FactoryOps_AccessManagementService.FactoryOpsApp.Common.CommonConstant;

namespace FactoryOperation_AccessManagementService.FactoryOpsApp.Infrastructure.Implementation.Repository.TenantAdmin.TenantAdminDashboard
{
    public class TenantAdminDashboardRepository : ITenantAdminDashboardRepository
    {
        private readonly TenantDbContextFactory _tenantDbContext;
        private readonly MasterFactoryOpsDbContext _masterDbcontext;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public TenantAdminDashboardRepository(TenantDbContextFactory tenantDbContext, MasterFactoryOpsDbContext masterDbcontext, IHttpContextAccessor httpContextAccessor)
        {
            _tenantDbContext = tenantDbContext;
            _masterDbcontext = masterDbcontext;
            _httpContextAccessor = httpContextAccessor;
        }

        public CountResponseModel GetUsersCount(int tenantId)
        {
            CountResponseModel response = new();
            try
            {
                using var tenantDb = _tenantDbContext.GetTenantDbContext(tenantId);
                int count = tenantDb.FactoryUsers
                    .Where(u => !u.IsDeleted)
                    .Count();

                response.StatusCode = StatusCode.Success;
                response.StatusMessage = TenantAdminDashboardStatusMessage.UsersFetched;
                response.Count = count;
            }
            catch (Exception ex)
            {
                response.StatusCode = StatusCode.Error;
                response.StatusMessage = ex.Message;
                response.Count = 0;
            }

            return response;
        }

        public CountResponseModel GetActiveUsersCountPresentMonth(int tenantId)
        {
            CountResponseModel response = new();
            try
            {
                using var tenantDb = _tenantDbContext.GetTenantDbContext(tenantId);
                DateTime utcNow = DateTime.UtcNow;
                DateTime monthStart = DateTime.SpecifyKind(new DateTime(utcNow.Year, utcNow.Month, 1), DateTimeKind.Utc);
                DateTime monthEnd = DateTime.SpecifyKind(monthStart.AddMonths(1), DateTimeKind.Utc);

                int count = tenantDb.FactoryUsers
                    .Where(u => !u.IsDeleted
                             && u.CreatedAt >= monthStart
                             && u.CreatedAt < monthEnd)
                    .Count();

                response.StatusCode = StatusCode.Success;
                response.StatusMessage = TenantAdminDashboardStatusMessage.ActiveUsersFetched;
                response.Count = count;
            }
            catch (Exception ex)
            {
                response.StatusCode = StatusCode.Error;
                response.StatusMessage = ex.Message;
                response.Count = 0;
            }

            return response;
        }

        public CountResponseModel GetActiveSupportTicketCount(int tenantId)
        {
            CountResponseModel response = new();
            try
            {
                using var tenantDb = _tenantDbContext.GetTenantDbContext(tenantId);
                int count = tenantDb.FactorySupportTickets
                    .Where(u => u.IsActive && !u.IsDeleted)
                    .Count();

                response.StatusCode = StatusCode.Success;
                response.StatusMessage = TenantAdminDashboardStatusMessage.ActiveTicketsFetched;
                response.Count = count;
            }
            catch (Exception ex)
            {
                response.StatusCode = StatusCode.Error;
                response.StatusMessage = ex.Message;
                response.Count = 0;
            }

            return response;
        }

        public CountResponseModel GetTotalTeamCount(int tenantId)
        {
            CountResponseModel response = new();
            try
            {
                using var tenantDb = _tenantDbContext.GetTenantDbContext(tenantId);

                int count = tenantDb.FactoryTeams.Count();

                response.StatusCode = StatusCode.Success;
                response.StatusMessage = TenantAdminDashboardStatusMessage.TeamsFetched;
                response.Count = count;
            }
            catch (Exception ex)
            {
                response.StatusCode = StatusCode.Error;
                response.StatusMessage = ex.Message;
                response.Count = 0;
            }

            return response;
        }

        public CountResponseModel GetSuspendUsersCount(int tenantId)
        {
            CountResponseModel response = new();
            try
            {
                using var tenantDb = _tenantDbContext.GetTenantDbContext(tenantId);

                int count = tenantDb.FactoryUsers.Where(u => u.Suspend)
                    .Count(); ;

                response.StatusCode = StatusCode.Success;
                response.StatusMessage = TenantAdminDashboardStatusMessage.SuspendedUsersFetched;
                response.Count = count;
            }
            catch (Exception ex)
            {
                response.StatusCode = StatusCode.Error;
                response.StatusMessage = ex.Message;
                response.Count = 0;
            }

            return response;
        }

        public CountResponseModel GetNewTicketCount(int tenantId)
        {
            CountResponseModel response = new();
            try
            {
                using var tenantDb = _tenantDbContext.GetTenantDbContext(tenantId);
                DateTime utcNow = DateTime.UtcNow;
                DateTime todayStart = utcNow.Date;
                DateTime todayEnd = todayStart.AddDays(1);

                int count = tenantDb.FactorySupportTickets
                    .Where(t => t.CreatedAt >= todayStart && t.CreatedAt < todayEnd && !t.IsDeleted)
                    .Count();

                response.StatusCode = StatusCode.Success;
                response.StatusMessage = TenantAdminDashboardStatusMessage.NewTicketsFetched;
                response.Count = count;
            }
            catch (Exception ex)
            {
                response.StatusCode = StatusCode.Error;
                response.StatusMessage = ex.Message;
                response.Count = 0;
            }

            return response;
        }

        public CountResponseModel GetTicketInProgressCount(int tenantId)
        {
            CountResponseModel response = new();
            try
            {
                using var tenantDb = _tenantDbContext.GetTenantDbContext(tenantId);

                int count = tenantDb.FactorySupportTickets
                    .Where(t => t.Status == "InProgress" && !t.IsDeleted)
                    .Count();

                response.StatusCode = StatusCode.Success;
                response.StatusMessage = TenantAdminDashboardStatusMessage.TicketsInProgressFetched;
                response.Count = count;
            }
            catch (Exception ex)
            {
                response.StatusCode = StatusCode.Success;
                response.StatusMessage = ex.Message;
                response.Count = 0;
            }

            return response;
        }

        public CountResponseModel GetTicketResolvedTodayCount(int tenantId)
        {
            CountResponseModel response = new();
            try
            {
                using var tenantDb = _tenantDbContext.GetTenantDbContext(tenantId);

                DateTime utcNow = DateTime.UtcNow;
                DateTime todayStart = utcNow.Date;
                DateTime todayEnd = todayStart.AddDays(1); // exclusive upper bound

                int count = tenantDb.FactorySupportTickets
                    .Where(t => t.Status == "Resolved" && t.CreatedAt >= todayStart && t.CreatedAt < todayEnd && !t.IsDeleted)
                    .Count();

                response.StatusCode = StatusCode.Success;
                response.StatusMessage = TenantAdminDashboardStatusMessage.TicketsResolvedFetched;
                response.Count = count;
            }
            catch (Exception ex)
            {
                response.StatusCode = StatusCode.Error;
                response.StatusMessage = ex.Message;
                response.Count = 0;
            }

            return response;
        }

        public CountResponseModel GetCriticalPriorityCount(int tenantId)
        {
            CountResponseModel response = new();
            try
            {
                using var tenantDb = _tenantDbContext.GetTenantDbContext(tenantId);

                int count = tenantDb.FactorySupportTickets
                    .Where(t => t.Priority == "Critical" && !t.IsDeleted)
                    .Count();

                response.StatusCode = StatusCode.Success;
                response.StatusMessage = TenantAdminDashboardStatusMessage.CriticalPriorityFetched;
                response.Count = count;
            }
            catch (Exception ex)
            {
                response.StatusCode = StatusCode.Error;
                response.StatusMessage = ex.Message;
                response.Count = 0;
            }

            return response;
        }

        public CountResponseModel GetUserByAdminCount(int tenantId)
        {
            CountResponseModel response = new();
            try
            {
                using var tenantDb = _tenantDbContext.GetTenantDbContext(tenantId);

                int count = tenantDb.FactoryUserRoles
                    .Where(u => u.FactoryRoles.RoleName.ToLower() == "admin" && !u.IsDeleted).Select(r => r.UserId).Distinct()
                    .Count();

                response.StatusCode = StatusCode.Success;
                response.StatusMessage = TenantAdminDashboardStatusMessage.AdminCountFetched;
                response.Count = count;
            }
            catch (Exception ex)
            {
                response.StatusCode = StatusCode.Error;
                response.StatusMessage = ex.Message;
                response.Count = 0;
            }

            return response;
        }

        public CountResponseModel GetUserBySupervisorCount(int tenantId)
        {
            CountResponseModel response = new();
            try
            {
                using var tenantDb = _tenantDbContext.GetTenantDbContext(tenantId);

                int count = tenantDb.FactoryUserRoles
                     .Where(u => u.FactoryRoles.RoleName.ToLower() == "supervisor" && !u.IsDeleted).Select(r => r.UserId).Distinct()
                     .Count();

                response.StatusCode = StatusCode.Success;
                response.StatusMessage = TenantAdminDashboardStatusMessage.SupervisorCountFetched;
                response.Count = count;
            }
            catch (Exception ex)
            {
                response.StatusCode = StatusCode.Error;
                response.StatusMessage = ex.Message;
                response.Count = 0;
            }

            return response;
        }

        public CountResponseModel GetUserByTechnicianCount(int tenantId)
        {
            CountResponseModel response = new();
            try
            {
                using var tenantDb = _tenantDbContext.GetTenantDbContext(tenantId);

                int count = tenantDb.FactoryUserRoles
                    .Where(u => u.FactoryRoles.RoleName.ToLower() == "technician" && !u.IsDeleted).Select(r => r.UserId).Distinct()
                    .Count();

                response.StatusCode = StatusCode.Success;
                response.StatusMessage = TenantAdminDashboardStatusMessage.TechnicianCountFetched;
                response.Count = count;
            }
            catch (Exception ex)
            {
                response.StatusCode = StatusCode.Error;
                response.StatusMessage = ex.Message;
                response.Count = 0;
            }

            return response;
        }

        public CountResponseModel GetUserByOperatorCount(int tenantId)
        {
            CountResponseModel response = new();
            try
            {
                using var tenantDb = _tenantDbContext.GetTenantDbContext(tenantId);

                int count = tenantDb.FactoryUserRoles
                    .Where(u => u.FactoryRoles.RoleName.ToLower() == "operator" && !u.IsDeleted).Select(r => r.UserId).Distinct()
                    .Count();

                response.StatusCode = StatusCode.Success;
                response.StatusMessage = TenantAdminDashboardStatusMessage.OperatorCountFetched;
                response.Count = count;
            }
            catch (Exception ex)
            {
                response.StatusCode = StatusCode.Error;
                response.StatusMessage = ex.Message;
                response.Count = 0;
            }

            return response;
        }

        public UserRoleCountResponseModel GetAllUserCount(int tenantId)
        {
            UserRoleCountResponseModel response = new();
            try
            {
                using var tenantDb = _tenantDbContext.GetTenantDbContext(tenantId);

                int AdminCount = tenantDb.FactoryUserRoles
                    .Where(u => u.FactoryRoles.RoleName.ToLower() == "admin" && !u.IsDeleted).Select(r => r.UserId).Distinct()
                    .Count();

                int SupervisorCount = tenantDb.FactoryUserRoles
                    .Where(u => u.FactoryRoles.RoleName.ToLower() == "supervisor" && !u.IsDeleted).Select(r => r.UserId).Distinct()
                    .Count();

                int OperatorCount = tenantDb.FactoryUserRoles
                    .Where(u => u.FactoryRoles.RoleName.ToLower() == "operator" && !u.IsDeleted).Select(r => r.UserId).Distinct()
                    .Count();

                int TechnicianCount = tenantDb.FactoryUserRoles
                    .Where(u => u.FactoryRoles.RoleName.ToLower() == "technician" && !u.IsDeleted).Select(r => r.UserId).Distinct()
                    .Count();

                response.StatusCode = StatusCode.Success;
                response.StatusMessage = TenantAdminDashboardStatusMessage.OperatorCountFetched;
                response.AdminCount = AdminCount;
                response.SupervisorCount = SupervisorCount;
                response.OperatorCount = OperatorCount;
                response.TechnicianCount = TechnicianCount;
            }
            catch (Exception ex)
            {
                response.StatusCode = StatusCode.Success;
                response.StatusMessage = ex.Message;
                response.AdminCount = 0;
                response.SupervisorCount = 0;
                response.OperatorCount = 0;
                response.TechnicianCount = 0;
            }

            return response;
        }

    }
}
