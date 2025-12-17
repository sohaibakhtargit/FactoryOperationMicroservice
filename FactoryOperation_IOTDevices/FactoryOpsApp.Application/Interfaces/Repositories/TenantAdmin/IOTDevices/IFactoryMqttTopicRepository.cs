using FactoryOperation_IOTDevices.FactoryOpsApp.Application.Common;
using FactoryOpsApp.Application.DTOs;
using FactoryOpsApp.Domain.Entities.FactoryOpsTenants;


namespace FactoryOperation_IOTDevices.FactoryOpsApp.Application.Interfaces.Repositories.TenantAdmin.IOTDevices
{
    public interface IFactoryMqttTopicRepository
    {
        Task<CommonResponseModel> AddMqttTopicAsync(MqttTopicDto topic);
        Task<GetSpecificRecord<FactoryMqttTopic?>> GetMqttTopicByIdAsync(int topicId, int tenantId);
        //Task<GetAllRecord<FactoryMqttTopic>> GetAllMqttTopicAsync(int tenantId);
        Task<GetAllRecord<MqttTopicDto>> GetAllMqttTopicAsync(int tenantId);
        Task<CommonResponseModel> UpdateMqttTopicAsync(MqttTopicDto topic);
        Task<CommonResponseModel> DeleteMqttTopicAsync(int topicId, int tenantId);
    }
}
