using FactoryOps_AccessManagementService.FactoryOpsApp.Application.Common;
using FactoryOpsApp.Application.DTOs;

namespace FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Repositories.TenantAdmin.TeamManagement
{
    public interface ITeamAlertNotificationRepository
    {
        Task<CommonResponseModel> AddTeamAlertNotificationAsync(CreateTeamAlertNotificationDto dto);
        Task<CommonResponseModel> UpdateTeamAlertNotificationAsync(UpdateTeamAlertNotificationDto dto);
        Task<CommonResponseModel> DeleteTeamAlertNotificationAsync(int teamAlertNotificationId, int tenantId);
        Task<GetAllRecord<GetTeamAlertNotificationDto>> GetAllTeamAlertNotificationsAsync(int tenantId);
        Task<GetSpecificRecord<GetTeamAlertNotificationDto>> GetTeamAlertNotificationByIdAsync(int teamAlertNotificationId, int tenantId);
        Task<CommonResponseModel> MarkAsReadAsync(int teamAlertNotificationId, int tenantId);
    }
}
