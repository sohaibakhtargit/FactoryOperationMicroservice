using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Repositories.TenantAdmin.TeamManagement;
using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.TeamManagement;
using FactoryOps_AccessManagementService.FactoryOpsApp.Application.Common;
using FactoryOpsApp.Application.DTOs;

namespace FactoryOperation_AccessManagementService.FactoryOpsApp.Infrastructure.Implementation.Service.TenantAdmin.TeamManagement
{
    public class UserBadgeService : IUserBadgeService
    {
        private readonly IUserBadgeRepository _repository;

        public UserBadgeService(IUserBadgeRepository repository)
        {
            _repository = repository;
        }

        public Task<CommonResponseModel> AddUserBadgeAsync(UserBadgeDto dto)
        {
            return _repository.AddUserBadgeAsync(dto);
        }

        public Task<CommonResponseModel> AwardUserBadgeAsync(AwardUserBadgeDto dto)
        {
            return _repository.AwardUserBadgeAsync(dto);
        }

        public Task<CommonResponseModel> UpdateUserBadgeProgressAsync(UpdateUserBadgeProgressDto dto)
        {
            return _repository.UpdateUserBadgeProgressAsync(dto);
        }

        public Task<CommonResponseModel> DeleteUserBadgeAsync(int userBadgeId, int tenantId)
        {
            return _repository.DeleteUserBadgeAsync(userBadgeId, tenantId);
        }

        public Task<GetAllRecord<GetUserBadgeDto>> GetAllUserBadgesAsync(int tenantId)
        {
            return _repository.GetAllUserBadgesAsync(tenantId);
        }

        public Task<GetSpecificRecord<GetUserBadgeDto>> GetUserBadgeByIdAsync(int userBadgeId, int tenantId)
        {
            return _repository.GetUserBadgeByIdAsync(userBadgeId, tenantId);
        }

        public Task<GetAllRecord<GetUserBadgeDto>> GetUserBadgesByUserAsync(int userId, int tenantId)
        {
            return _repository.GetUserBadgesByUserAsync(userId, tenantId);
        }

        public Task<GetAllRecord<GetUserBadgeDto>> GetUserBadgesByBadgeAsync(int badgeId, int tenantId)
        {
            return _repository.GetUserBadgesByBadgeAsync(badgeId, tenantId);
        }
    }
}