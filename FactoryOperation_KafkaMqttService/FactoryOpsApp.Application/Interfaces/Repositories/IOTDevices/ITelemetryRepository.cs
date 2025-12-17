using FactoryOperation_KafkaMqttService.FactoryOpsApp.Application.Common;
using FactoryOpsApp.Application.DTOs;
using FactoryOpsApp.Domain.Entities.FactoryOpsTenants;

namespace FactoryOperation_KafkaMqttService.FactoryOpsApp.Application.Interfaces.Repositories.IOTDevices
{
    public interface ITelemetryRepository
    {
        Task<CommonResponseModel> AddTelemetryAsync(Telemetry telemetry, int tenantId);
        Task<GetAllRecord<TelemetryResponseDto>> GetTelemetryByDeviceIdAsync(int deviceId, int tenantId);
        Task<GetAllRecord<DeviceStatusLog>> GetDeviceStatusLogsAsync(int deviceId, int tenantId, int limit = 10);
    }
}
