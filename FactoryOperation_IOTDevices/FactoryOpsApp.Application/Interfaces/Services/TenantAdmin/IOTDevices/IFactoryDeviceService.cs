using FactoryOperation_IOTDevices.FactoryOpsApp.Application.Common;
using FactoryOpsApp.Application.DTOs;


namespace FactoryOpsApp_IOTDevices.FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.IOTDevices
{
    public interface IFactoryDeviceService
    {
        Task<CommonResponseModel> AddDeviceAsync(DeviceDto dto);
        Task<CommonResponseModel> UpdateDeviceAsync(DeviceDto dto);
        Task<CommonResponseModel> DeleteDeviceAsync(int deviceId, int tenantId);
        Task<GetAllRecord<GetDeviceDto>> GetAllDevicesAsync(int tenantId);
        Task<GetSpecificRecord<GetDeviceDto>> GetDeviceByIdAsync(int deviceId, int tenantId);
        Task<GetAllRecord<GetDeviceDto>> GetAllDevicesFromAllTenantsAsync(int deviceId, string deviceCode);
    }
}
