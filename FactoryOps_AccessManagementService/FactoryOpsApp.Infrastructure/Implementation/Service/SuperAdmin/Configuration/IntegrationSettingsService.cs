using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Repositories.SuperAdmin.Configuration;
using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Services.SuperAdmin.Configuration;
using FactoryOps_AccessManagementService.FactoryOpsApp.Application.Common;
using FactoryOpsApp.Application.DTOs;
using FactoryOpsApp.Domain.Entities;

namespace FactoryOperation_AccessManagementService.FactoryOpsApp.Infrastructure.Implementation.Service.SuperAdmin.Configuration
{
    public class IntegrationSettingsService : IIntegrationSettingsService
    {
        private readonly IIntegrationSettingsRepository _repository;

        public IntegrationSettingsService(IIntegrationSettingsRepository repository)
        {
            _repository = repository;
        }

        public Task<GetSpecificRecord<IntegrationSettingsDto>> GetIntegrationSettingByIdAsync(int IntegrationId, int tenantId)
            => _repository.GetIntegrationSettingByIdAsync(IntegrationId, tenantId);
        public Task<GetAllRecord<IntegrationSettingsDto>> GetAllIntegrationSettingAsync(int tenantId)
            => _repository.GetAllIntegrationSettingAsync(tenantId);
        public Task<GetAllRecord<IntegrationSettingsDto>> GetIntegrationSettingByCategoryAsync(IntegrationSettingsCategory category, int tenantId)
            => _repository.GetIntegrationSettingByCategoryAsync(category, tenantId);
        public Task<CommonResponseModel> AddIntegrationSettingAsync(CreateIntegrationSettingsDto entity)
            => _repository.AddIntegrationSettingAsync(entity);
        public Task<CommonResponseModel> UpdateIntegrationSettingAsync(UpdateIntegrationSettingsDto entity, int tenantId)
            => _repository.UpdateIntegrationSettingAsync(entity, tenantId);
        public Task<CommonResponseModel> DeleteIntegrationSettingAsync(int IntegrationId, int deletedBy, int tenantId)
            => _repository.DeleteIntegrationSettingAsync(IntegrationId, deletedBy, tenantId);
    }
}
