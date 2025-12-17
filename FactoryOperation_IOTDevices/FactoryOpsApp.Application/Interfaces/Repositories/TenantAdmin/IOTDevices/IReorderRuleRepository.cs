using FactoryOperation_IOTDevices.FactoryOpsApp.Application.Common;
using FactoryOpsApp.Application.DTOs;


namespace FactoryOperation_IOTDevices.FactoryOpsApp.Application.Interfaces.Repositories.TenantAdmin.IOTDevices
{
    public interface IReorderRuleRepository
    {
        Task<CommonResponseModel> CreateAsync(CreateReorderRuleDto dto);
        Task<CommonResponseModel> UpdateAsync(UpdateReorderRuleDto dto);
        Task<CommonResponseModel> DeleteAsync(int tenantId, int id, int deletedBy);
        Task<GetAllRecord<ReorderRuleResponseDto>> GetAllAsync(int tenantId);
        Task<GetSpecificRecord<ReorderRuleResponseDto>> GetByIdAsync(int tenantId, int id);
        Task<GetSpecificRecord<AutomatedReplenishmentDashboardDto>> GetDashboardDataAsync(int tenantId);
    }
}
