using FactoryOps_AccessManagementService.FactoryOpsApp.Application.Common;
using FactoryOpsApp.Application.DTOs;

namespace FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Repositories.TenantAdmin.TenantAdminManagement
{
    public interface IFactoryNotificationRulesRepository
    {
        public Task<CommonResponseModel> CreateAsync(NotificationRuleDto dto);
        public Task<GetAllRecord<NotificationRuleDto>> GetAllAsync(int tenantId);
        public Task<GetSpecificRecord<NotificationRuleDto>> GetByIdAsync(int id, int tenantId);
        public Task<CommonResponseModel> UpdateAsync(NotificationRuleDto dto);
        public Task<CommonResponseModel> DeleteAsync(int id, int tenantId);
    }
}
