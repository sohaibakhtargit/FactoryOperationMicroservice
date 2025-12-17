using FactoryOperation_IOTDevices.FactoryOpsApp.Application.Common;
using FactoryOpsApp.Application.DTOs;
using FactoryOpsApp.Domain.Entities.FactoryOpsTenants;

namespace FactoryOpsApp_IOTDevices.FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.IOTDevices
{
    public interface IFactoryMqttTopicService
    {

        Task<CommonResponseModel> AddMqttTopicAsync(MqttTopicDto topic);
        Task<GetSpecificRecord<FactoryMqttTopic?>> GetMqttTopicByIdAsync(int topicId, int tenantId);
        Task<GetAllRecord<MqttTopicDto>> GetAllMqttTopicAsync(int tenantId);
        Task<CommonResponseModel> UpdateMqttTopicAsync(MqttTopicDto topic);
        Task<CommonResponseModel> DeleteMqttTopicAsync(int topicId, int tenantId);
    }
}
