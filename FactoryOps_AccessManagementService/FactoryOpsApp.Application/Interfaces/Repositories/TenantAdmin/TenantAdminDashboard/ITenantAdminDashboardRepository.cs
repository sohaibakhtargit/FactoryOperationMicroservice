using FactoryOps_AccessManagementService.FactoryOpsApp.Application.Common;

namespace FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Repositories.TenantAdmin.TenantAdminDashboard
{
    public interface ITenantAdminDashboardRepository
    {
      

        CountResponseModel GetUsersCount(int tenantId);

        CountResponseModel GetActiveUsersCountPresentMonth(int tenantId);

        CountResponseModel GetActiveSupportTicketCount(int tenantId);

        CountResponseModel GetTotalTeamCount(int tenantId);

        CountResponseModel GetSuspendUsersCount(int tenantId);

        CountResponseModel GetNewTicketCount(int tenantId);

        CountResponseModel GetTicketInProgressCount(int tenantId);

        CountResponseModel GetTicketResolvedTodayCount(int tenantId);

        CountResponseModel GetCriticalPriorityCount(int tenantId);

        CountResponseModel GetUserByAdminCount(int tenantId);

        CountResponseModel GetUserBySupervisorCount(int tenantId);

        CountResponseModel GetUserByTechnicianCount(int tenantId);

        CountResponseModel GetUserByOperatorCount(int tenantId);

        UserRoleCountResponseModel GetAllUserCount(int tenantId);
    }
}
