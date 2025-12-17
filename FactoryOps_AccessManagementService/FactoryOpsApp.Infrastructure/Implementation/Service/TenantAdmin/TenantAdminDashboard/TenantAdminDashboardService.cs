using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Repositories.TenantAdmin.TenantAdminDashboard;
using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.TenantAdminDashboard;
using FactoryOps_AccessManagementService.FactoryOpsApp.Application.Common;

namespace FactoryOperation_AccessManagementService.FactoryOpsApp.Infrastructure.Implementation.Service.TenantAdmin.TenantAdminDashboard
{
    public class TenantAdminDashboardService : ITenantAdminDashboardService
    {
        private readonly ITenantAdminDashboardRepository _repository;

        public TenantAdminDashboardService(ITenantAdminDashboardRepository repository)
        {
            _repository = repository;
        }

        public CountResponseModel GetUsersCount(int tenantId)
        {
            return _repository.GetUsersCount(tenantId);
        }

        public CountResponseModel GetActiveUsersCountPresentMonth(int tenantId)
        {
            return _repository.GetActiveUsersCountPresentMonth(tenantId);
        }

        public CountResponseModel GetActiveSupportTicketCount(int tenantId)
        {
            return _repository.GetActiveSupportTicketCount(tenantId);
        }

        public CountResponseModel GetTotalTeamCount(int tenantId)
        {
            return _repository.GetTotalTeamCount(tenantId);
        }

        public CountResponseModel GetSuspendUsersCount(int tenantId)
        {
            return _repository.GetSuspendUsersCount(tenantId);
        }

        public CountResponseModel GetNewTicketCount(int tenantId)
        {
            return _repository.GetNewTicketCount(tenantId);
        }

        public CountResponseModel GetTicketInProgressCount(int tenantId)
        {
            return _repository.GetTicketInProgressCount(tenantId);
        }

        public CountResponseModel GetTicketResolvedTodayCount(int tenantId)
        {
            return _repository.GetTicketResolvedTodayCount(tenantId);
        }

        public CountResponseModel GetCriticalPriorityCount(int tenantId)
        {
            return _repository.GetCriticalPriorityCount(tenantId);
        }

        public CountResponseModel GetUserByAdminCount(int tenantId)
        {
            return _repository.GetUserByAdminCount(tenantId);
        }

        public CountResponseModel GetUserBySupervisorCount(int tenantId)
        {
            return _repository.GetUserBySupervisorCount(tenantId);
        }

        public CountResponseModel GetUserByTechnicianCount(int tenantId)
        {
            return _repository.GetUserByTechnicianCount(tenantId);
        }

        public CountResponseModel GetUserByOperatorCount(int tenantId)
        {
            return _repository.GetUserByOperatorCount(tenantId);
        }

        public UserRoleCountResponseModel GetAllUserCount(int tenantId)
        {
            return _repository.GetAllUserCount(tenantId);
        }
    }
}
