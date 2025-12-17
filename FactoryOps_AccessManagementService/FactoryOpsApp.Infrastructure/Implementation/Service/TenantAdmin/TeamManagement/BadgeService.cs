using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Repositories.TenantAdmin.TeamManagement;
using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.TeamManagement;
using FactoryOps_AccessManagementService.FactoryOpsApp.Application.Common;
using FactoryOpsApp.Application.DTOs;

namespace FactoryOperation_AccessManagementService.FactoryOpsApp.Infrastructure.Implementation.Service.TenantAdmin.TeamManagement
{
    public class BadgeService : IBadgeService
    {
        private readonly IBadgeRepository _repository;

        public BadgeService(IBadgeRepository repository)
        {
            _repository = repository;
        }

        public Task<CommonResponseModel> AddBadgeAsync(BadgeDto dto)
        {
            return _repository.AddBadgeAsync(dto);
        }

        public Task<CommonResponseModel> UpdateBadgeAsync(BadgeDto dto)
        {
            return _repository.UpdateBadgeAsync(dto);
        }

        public Task<CommonResponseModel> DeleteBadgeAsync(int badgeId, int tenantId)
        {
            return _repository.DeleteBadgeAsync(badgeId, tenantId);
        }

        public Task<GetAllRecord<GetBadgeDto>> GetAllBadgesAsync(int tenantId)
        {
            return _repository.GetAllBadgesAsync(tenantId);
        }

        public Task<GetSpecificRecord<GetBadgeDto>> GetBadgeByIdAsync(int badgeId, int tenantId)
        {
            return _repository.GetBadgeByIdAsync(badgeId, tenantId);
        }
    }
}
