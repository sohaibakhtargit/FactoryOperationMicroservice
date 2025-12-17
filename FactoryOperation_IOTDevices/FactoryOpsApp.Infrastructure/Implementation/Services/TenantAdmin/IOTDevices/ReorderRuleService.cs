using FactoryOperation_IOTDevices.FactoryOpsApp.Application.Common;
using FactoryOperation_IOTDevices.FactoryOpsApp.Application.Interfaces.Repositories.TenantAdmin.IOTDevices;
using FactoryOpsApp.Application.DTOs;
using FactoryOpsApp_IOTDevices.FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.IOTDevices;

namespace FactoryOperation_IOTDevices.FactoryOpsApp.Infrastructure.Implementation.Services.TenantAdmin.IOTDevices
{
    public class ReorderRuleService : IReorderRuleService
    {
        private readonly IReorderRuleRepository _reorderRuleRepository;

        public ReorderRuleService(IReorderRuleRepository reorderRuleRepository)
        {
            _reorderRuleRepository = reorderRuleRepository;
        }

        public Task<CommonResponseModel> CreateAsync(CreateReorderRuleDto dto)
            => _reorderRuleRepository.CreateAsync(dto);

        public Task<CommonResponseModel> UpdateAsync(UpdateReorderRuleDto dto)
            => _reorderRuleRepository.UpdateAsync(dto);

        public Task<CommonResponseModel> DeleteAsync(int tenantId, int id, int deletedBy)
            => _reorderRuleRepository.DeleteAsync(tenantId, id, deletedBy);

        public Task<GetAllRecord<ReorderRuleResponseDto>> GetAllAsync(int tenantId)
            => _reorderRuleRepository.GetAllAsync(tenantId);

        public Task<GetSpecificRecord<ReorderRuleResponseDto>> GetByIdAsync(int tenantId, int id)
            => _reorderRuleRepository.GetByIdAsync(tenantId, id);

        public Task<GetSpecificRecord<AutomatedReplenishmentDashboardDto>> GetDashboardDataAsync(int tenantId)
            => _reorderRuleRepository.GetDashboardDataAsync(tenantId);
    }
}
