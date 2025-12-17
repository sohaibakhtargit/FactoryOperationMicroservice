using FactoryOperation_IOTDevices.FactoryOpsApp.Application.Common;
using FactoryOperation_IOTDevices.FactoryOpsApp.Application.Interfaces.Repositories.TenantAdmin.IOTDevices;
using FactoryOpsApp.Application.DTOs;
using FactoryOpsApp_IOTDevices.FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.IOTDevices;

namespace FactoryOperation_IOTDevices.FactoryOpsApp.Infrastructure.Implementation.Services.TenantAdmin.IOTDevices
{
    public class FactoryDeviceService : IFactoryDeviceService
    {
        private readonly IFactoryDeviceRepository _repository;

        public FactoryDeviceService(IFactoryDeviceRepository repository)
        {
            _repository = repository;
        }

        public Task<CommonResponseModel> AddDeviceAsync(DeviceDto dto)
        {
            return _repository.AddDeviceAsync(dto);
        }

        public Task<CommonResponseModel> UpdateDeviceAsync(DeviceDto dto)
        {
            return _repository.UpdateDeviceAsync(dto);
        }

        public Task<CommonResponseModel> DeleteDeviceAsync(int deviceId, int tenantId)
        {
            return _repository.DeleteDeviceAsync(deviceId, tenantId);
        }

        public Task<GetAllRecord<GetDeviceDto>> GetAllDevicesAsync(int tenantId)
        {
            return _repository.GetAllDevicesAsync(tenantId);
        }

        public Task<GetSpecificRecord<GetDeviceDto>> GetDeviceByIdAsync(int deviceId, int tenantId)
        {
            return _repository.GetDeviceByIdAsync(deviceId, tenantId);
        }

        public Task<GetAllRecord<GetDeviceDto>> GetAllDevicesFromAllTenantsAsync(int deviceId, string deviceCode)
        {
            return _repository.GetAllDevicesFromAllTenantsAsync(deviceId, deviceCode);
        }
    }
}
