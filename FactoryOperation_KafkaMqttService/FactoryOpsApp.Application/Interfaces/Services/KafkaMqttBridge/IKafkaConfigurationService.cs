using FactoryOperation_KafkaMqttService.FactoryOpsApp.Application.Common;
using FactoryOperation_KafkaMqttService.FactoryOpsApp.Application.DTOs.KafkaMqttBridge;

namespace FactoryOperation_KafkaMqttService.FactoryOpsApp.Application.Interfaces.Services.KafkaMqttBridge
{
    public interface IKafkaConfigurationService
    {
        Task<GetAllRecord<KafkaConfigurationDto>> GetAllAsync(int tenantId);
        Task<GetSpecificRecord<KafkaConfigurationDto>> GetByIdAsync(int id, int tenantId);
        Task<CommonResponseModel> CreateAsync(KafkaConfigurationCreateDto dto);
        Task<CommonResponseModel> UpdateAsync(KafkaConfigurationUpdateDto dto);
        Task<CommonResponseModel> DeleteAsync(int id, int tenantId);
    }
}
