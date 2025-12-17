using FactoryOperation_KafkaMqttService.FactoryOpsApp.Application.Common;
using FactoryOperation_KafkaMqttService.FactoryOpsApp.Application.DTOs.KafkaMqttBridge;
using FactoryOperation_KafkaMqttService.FactoryOpsApp.Application.Interfaces.Repositories.KafkaMqttBridge;
using FactoryOperation_KafkaMqttService.FactoryOpsApp.Application.Interfaces.Services.KafkaMqttBridge;

namespace FactoryOperation_KafkaMqttService.FactoryOpsApp.Infrastructure.Implementations.Services.KafkaMqttBridge
{
    public class KafkaConfigurationService : IKafkaConfigurationService
    {
        private readonly IKafkaConfigurationRepository _repo;
        public KafkaConfigurationService(IKafkaConfigurationRepository repo) => _repo = repo;

        public Task<GetAllRecord<KafkaConfigurationDto>> GetAllAsync(int tenantId) => _repo.GetAllAsync(tenantId);
        public Task<GetSpecificRecord<KafkaConfigurationDto>> GetByIdAsync(int id, int tenantId) => _repo.GetByIdAsync(id, tenantId);
        public Task<CommonResponseModel> CreateAsync(KafkaConfigurationCreateDto dto) => _repo.CreateAsync(dto);
        public Task<CommonResponseModel> UpdateAsync(KafkaConfigurationUpdateDto dto) => _repo.UpdateAsync(dto);
        public Task<CommonResponseModel> DeleteAsync(int id, int tenantId) => _repo.DeleteAsync(id, tenantId);
    }
}
