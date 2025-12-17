using FactoryOperation_KafkaMqttService.FactoryOpsApp.Application.Common;
using FactoryOperation_KafkaMqttService.FactoryOpsApp.Application.DTOs.KafkaMqttBridge;

namespace FactoryOperation_KafkaMqttService.FactoryOpsApp.Application.Interfaces.Repositories.KafkaMqttBridge
{
    public interface IBridgeConfigurationRepository
    {
        Task<GetAllRecord<BridgeConfigurationDto>> GetAllAsync(int tenantId);
        Task<GetSpecificRecord<BridgeConfigurationDto>> GetByIdAsync(int id, int tenantId);
        Task<CommonResponseModel> CreateAsync(BridgeConfigurationCreateDto dto);
        Task<CommonResponseModel> UpdateAsync(BridgeConfigurationUpdateDto dto);
        Task<CommonResponseModel> DeleteAsync(int id, int tenantId);
    }
}
