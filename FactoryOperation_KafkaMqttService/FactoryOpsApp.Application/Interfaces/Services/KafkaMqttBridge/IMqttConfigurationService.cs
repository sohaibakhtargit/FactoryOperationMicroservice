using FactoryOperation_KafkaMqttService.FactoryOpsApp.Application.Common;
using FactoryOperation_KafkaMqttService.FactoryOpsApp.Application.DTOs.KafkaMqttBridge;

namespace FactoryOperation_KafkaMqttService.FactoryOpsApp.Application.Interfaces.Services.KafkaMqttBridge
{
    public interface IMqttConfigurationService
    {
        Task<GetAllRecord<MqttConfigurationDto>> GetAllAsync(int tenantId);
        Task<GetSpecificRecord<MqttConfigurationDto>> GetByIdAsync(int id, int tenantId);
        Task<CommonResponseModel> CreateAsync(MqttConfigurationCreateDto dto);
        Task<CommonResponseModel> UpdateAsync(MqttConfigurationUpdateDto dto);
        Task<CommonResponseModel> DeleteAsync(int id, int tenantId);
    }
}
