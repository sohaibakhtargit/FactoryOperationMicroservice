using FactoryOperation_IOTDevices.FactoryOpsApp.Application.Common;
using FactoryOpsApp.Application.DTOs;
using FactoryOpsApp.Domain.Entities.FactoryOpsTenants;

namespace FactoryOperation_IOTDevices.FactoryOpsApp.Application.Interfaces.Repositories.TenantAdmin.IOTDevices
{
    public interface IAlertRuleRepository
    {
        Task<CommonResponseModel> AddAlertRuleAsync(AlertRuleDto dto);
        Task<CommonResponseModel> UpdateAlertRuleAsync(AlertRuleDto dto);
        Task<CommonResponseModel> DeleteAlertRuleAsync(int alertRuleId, int tenantId);
        Task<GetAllRecord<GetAlertRuleDto>> GetAllAlertRulesAsync(int tenantId, AlertStatusEnum? statusFilter = null);
        Task<GetSpecificRecord<GetAlertRuleDto>> GetAlertRuleByIdAsync(int alertRuleId, int tenantId);
    }
}