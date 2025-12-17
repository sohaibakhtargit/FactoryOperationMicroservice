using FactoryOperation_KafkaMqttService.FactoryOpsApp.Application.Common;
using FactoryOperation_KafkaMqttService.FactoryOpsApp.Application.DTOs.KafkaMqttBridge;
using FactoryOperation_KafkaMqttService.FactoryOpsApp.Application.Interfaces.Repositories.KafkaMqttBridge;
using FactoryOperation_KafkaMqttService.FactoryOpsApp.Application.Interfaces.Services.KafkaMqttBridge;

namespace FactoryOperation_KafkaMqttService.FactoryOpsApp.Infrastructure.Implementations.Services.KafkaMqttBridge
{
    public class MqttConfigurationService : IMqttConfigurationService
    {
        private readonly IMqttConfigurationRepository _repo;
        public MqttConfigurationService(IMqttConfigurationRepository repo) => _repo = repo;

        public Task<GetAllRecord<MqttConfigurationDto>> GetAllAsync(int tenantId) => _repo.GetAllAsync(tenantId);
        public Task<GetSpecificRecord<MqttConfigurationDto>> GetByIdAsync(int id, int tenantId) => _repo.GetByIdAsync(id, tenantId);
        public Task<CommonResponseModel> CreateAsync(MqttConfigurationCreateDto dto) => _repo.CreateAsync(dto);
        public Task<CommonResponseModel> UpdateAsync(MqttConfigurationUpdateDto dto) => _repo.UpdateAsync(dto);
        public Task<CommonResponseModel> DeleteAsync(int id, int tenantId) => _repo.DeleteAsync(id, tenantId);
    }
}
