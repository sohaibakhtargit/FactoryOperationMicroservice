using FactoryOperation_KafkaMqttService.FactoryOpsApp.Application.Common;
using FactoryOperation_KafkaMqttService.FactoryOpsApp.Application.DTOs.KafkaMqttBridge;
using FactoryOperation_KafkaMqttService.FactoryOpsApp.Application.Interfaces.Repositories.KafkaMqttBridge;
using FactoryOperation_KafkaMqttService.FactoryOpsApp.Application.Interfaces.Services.KafkaMqttBridge;

namespace FactoryOperation_KafkaMqttService.FactoryOpsApp.Infrastructure.Implementations.Services.KafkaMqttBridge
{
    public class BridgeConfigurationService : IBridgeConfigurationService
    {
        private readonly IBridgeConfigurationRepository _repo;
        public BridgeConfigurationService(IBridgeConfigurationRepository repo) => _repo = repo;

        public Task<GetAllRecord<BridgeConfigurationDto>> GetAllAsync(int tenantId) => _repo.GetAllAsync(tenantId);
        public Task<GetSpecificRecord<BridgeConfigurationDto>> GetByIdAsync(int id, int tenantId) => _repo.GetByIdAsync(id, tenantId);
        public Task<CommonResponseModel> CreateAsync(BridgeConfigurationCreateDto dto) => _repo.CreateAsync(dto);
        public Task<CommonResponseModel> UpdateAsync(BridgeConfigurationUpdateDto dto) => _repo.UpdateAsync(dto);
        public Task<CommonResponseModel> DeleteAsync(int id, int tenantId) => _repo.DeleteAsync(id, tenantId);
    }
}
