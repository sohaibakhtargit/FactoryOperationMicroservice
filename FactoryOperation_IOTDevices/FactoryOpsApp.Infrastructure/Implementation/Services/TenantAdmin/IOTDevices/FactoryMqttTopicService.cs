using FactoryOperation_IOTDevices.FactoryOpsApp.Application.Common;
using FactoryOperation_IOTDevices.FactoryOpsApp.Application.Interfaces.Repositories.TenantAdmin.IOTDevices;
using FactoryOpsApp.Application.DTOs;
using FactoryOpsApp_IOTDevices.FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.IOTDevices;
using FactoryOpsApp.Domain.Entities.FactoryOpsTenants;

namespace FactoryOperation_IOTDevices.FactoryOpsApp.Infrastructure.Implementation.Services.TenantAdmin.IOTDevices
{
    public class FactoryMqttTopicService : IFactoryMqttTopicService
    {
        private readonly IFactoryMqttTopicRepository _repository;

        public FactoryMqttTopicService(IFactoryMqttTopicRepository repository)
        {
            _repository = repository;
        }
        public Task<CommonResponseModel> AddMqttTopicAsync(MqttTopicDto topic)
        {
            return _repository.AddMqttTopicAsync(topic);
        }

        public Task<CommonResponseModel> DeleteMqttTopicAsync(int topicId, int tenantId)
        {
            return _repository.DeleteMqttTopicAsync(topicId, tenantId);
        }

        public Task<GetAllRecord<MqttTopicDto>> GetAllMqttTopicAsync(int tenantId)
        {
            return _repository.GetAllMqttTopicAsync(tenantId);
        }

        public Task<GetSpecificRecord<FactoryMqttTopic?>> GetMqttTopicByIdAsync(int topicId, int tenantId)
        {
            return _repository.GetMqttTopicByIdAsync(topicId, tenantId);
        }

        public Task<CommonResponseModel> UpdateMqttTopicAsync(MqttTopicDto topic)
        {
            return _repository.UpdateMqttTopicAsync(topic);
        }
    }
}
