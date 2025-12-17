using FactoryOps_AccessManagementService.FactoryOpsApp.Application.Common;
using FactoryOpsApp.Application.DTOs;
using FactoryOpsApp.Domain.Entities;
namespace FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Services.SuperAdmin.Configuration
{
    public interface IIntegrationSettingsService
    {
        Task<GetSpecificRecord<IntegrationSettingsDto>> GetIntegrationSettingByIdAsync(int IntegrationId, int tenantId);
        Task<GetAllRecord<IntegrationSettingsDto>> GetAllIntegrationSettingAsync(int tenantId);
        Task<GetAllRecord<IntegrationSettingsDto>> GetIntegrationSettingByCategoryAsync(IntegrationSettingsCategory category, int tenantId);
        Task<CommonResponseModel> AddIntegrationSettingAsync(CreateIntegrationSettingsDto entity);
        Task<CommonResponseModel> UpdateIntegrationSettingAsync(UpdateIntegrationSettingsDto entity, int tenantId);
        Task<CommonResponseModel> DeleteIntegrationSettingAsync(int IntegrationId, int deletedBy, int tenantId);
    }
}
