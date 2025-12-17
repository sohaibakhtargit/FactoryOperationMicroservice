using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Repositories.TenantAdmin.TeamManagement;
using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.TeamManagement;
using FactoryOps_AccessManagementService.FactoryOpsApp.Application.Common;
using FactoryOpsApp.Application.DTOs;

namespace FactoryOperation_AccessManagementService.FactoryOpsApp.Infrastructure.Implementation.Service.TenantAdmin.TeamManagement
{
    public class TeamAlertNotificationService : ITeamAlertNotificationService
    {
        private readonly ITeamAlertNotificationRepository _repository;

        public TeamAlertNotificationService(ITeamAlertNotificationRepository repository)
        {
            _repository = repository;
        }

        public Task<CommonResponseModel> AddTeamAlertNotificationAsync(CreateTeamAlertNotificationDto dto)
        {
            return _repository.AddTeamAlertNotificationAsync(dto);
        }

        public Task<CommonResponseModel> UpdateTeamAlertNotificationAsync(UpdateTeamAlertNotificationDto dto)
        {
            return _repository.UpdateTeamAlertNotificationAsync(dto);
        }

        public Task<CommonResponseModel> DeleteTeamAlertNotificationAsync(int teamAlertNotificationId, int tenantId)
        {
            return _repository.DeleteTeamAlertNotificationAsync(teamAlertNotificationId, tenantId);
        }

        public Task<GetAllRecord<GetTeamAlertNotificationDto>> GetAllTeamAlertNotificationsAsync(int tenantId)
        {
            return _repository.GetAllTeamAlertNotificationsAsync(tenantId);
        }

        public Task<GetSpecificRecord<GetTeamAlertNotificationDto>> GetTeamAlertNotificationByIdAsync(int teamAlertNotificationId, int tenantId)
        {
            return _repository.GetTeamAlertNotificationByIdAsync(teamAlertNotificationId, tenantId);
        }

        public Task<CommonResponseModel> MarkAsReadAsync(int teamAlertNotificationId, int tenantId)
        {
            return _repository.MarkAsReadAsync(teamAlertNotificationId, tenantId);
        }
    }
}