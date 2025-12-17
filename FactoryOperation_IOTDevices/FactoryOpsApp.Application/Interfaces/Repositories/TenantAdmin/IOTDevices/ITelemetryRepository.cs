
using FactoryOperation_IOTDevices.FactoryOpsApp.Application.Common;
using FactoryOpsApp.Application.DTOs;
using FactoryOpsApp.Domain.Entities.FactoryOpsTenants;

namespace FactoryOperation_IOTDevices.FactoryOpsApp.Application.Interfaces.Repositories.TenantAdmin.IOTDevices
{
    public interface ITelemetryRepository
    {
        Task<CommonResponseModel> AddTelemetryAsync(Telemetry telemetry, int tenantId);
        Task<GetAllRecord<TelemetryResponseDto>> GetTelemetryByDeviceIdAsync(int deviceId, int tenantId);
        Task<GetAllRecord<DeviceStatusLog>> GetDeviceStatusLogsAsync(int deviceId, int tenantId, int limit = 10);
    }
}
