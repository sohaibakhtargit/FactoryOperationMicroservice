using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Repositories.TenantAdmin.TenantAdminManagement;
using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.TenantAdminManagement;
using FactoryOps_AccessManagementService.FactoryOpsApp.Application.Common;
using FactoryOpsApp.Application.DTOs;

namespace FactoryOperation_AccessManagementService.FactoryOpsApp.Infrastructure.Implementation.Service.TenantAdmin.TenantAdminManagement
{
    public class FactoryNotificationRulesService : IFactoryNotificationRulesService
    {

        private readonly IFactoryNotificationRulesRepository _repository;

        public FactoryNotificationRulesService(IFactoryNotificationRulesRepository repository)
        {
            _repository = repository;
        }

        public async Task<CommonResponseModel> CreateAsync(NotificationRuleDto dto)
        {
            return await _repository.CreateAsync(dto);
        }
        public Task<GetAllRecord<NotificationRuleDto>> GetAllAsync(int tenantId)
        {
            return _repository.GetAllAsync(tenantId);
        }
        public Task<GetSpecificRecord<NotificationRuleDto>> GetByIdAsync(int id, int tenantId)
        {
            return _repository.GetByIdAsync(id, tenantId);
        }
        public async Task<CommonResponseModel> UpdateAsync(NotificationRuleDto dto)
        {
            return await _repository.UpdateAsync(dto);
        }

        public async Task<CommonResponseModel> DeleteAsync(int id, int tenantId)
        {
            return await _repository.DeleteAsync(id, tenantId);
        }


    }
}
