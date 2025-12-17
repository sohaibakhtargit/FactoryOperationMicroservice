using FactoryOperation_IOTDevices.FactoryOpsApp.Application.Common;
using FactoryOpsApp.Application.DTOs;

namespace FactoryOpsApp_IOTDevices.FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.IOTDevices
{
    public interface IReorderRuleService
    {
        Task<CommonResponseModel> CreateAsync(CreateReorderRuleDto dto);
        Task<CommonResponseModel> UpdateAsync(UpdateReorderRuleDto dto);
        Task<CommonResponseModel> DeleteAsync(int tenantId, int id, int deletedBy);
        Task<GetAllRecord<ReorderRuleResponseDto>> GetAllAsync(int tenantId);
        Task<GetSpecificRecord<ReorderRuleResponseDto>> GetByIdAsync(int tenantId, int id);
        Task<GetSpecificRecord<AutomatedReplenishmentDashboardDto>> GetDashboardDataAsync(int tenantId);
    }
}
