using FactoryOperation_IOTDevices.FactoryOpsApp.Application.Common;
using FactoryOperation_IOTDevices.FactoryOpsApp.Application.Interfaces.Repositories.TenantAdmin.IOTDevices;
using FactoryOpsApp.Application.DTOs;
using FactoryOpsApp.Domain.Entities.FactoryOpsTenants;
using FactoryOpsApp_IOTDevices.FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.IOTDevices;


namespace FactoryOperation_IOTDevices.FactoryOpsApp.Infrastructure.Implementation.Services.TenantAdmin.IOTDevices
{
    public class DeviceConfigurationService : IDeviceConfigurationService
    {
        private readonly IDeviceConfigurationRepository _repository;

        public DeviceConfigurationService(IDeviceConfigurationRepository repository)
        {
            _repository = repository;
        }

        public async Task<DeviceConfigurationDto> AddAsync(DeviceConfigurationDto model)
        {
            return await _repository.AddAsync(model);
        }

        public async Task<DeviceConfigurationDto> UpdateAsync(DeviceConfigurationDto model)
        {
            return await _repository.UpdateAsync(model);
        }

        public async Task<DeviceConfiguration?> GetByIdAsync(int tenantId, int deviceId, int configId)
        {
            return await _repository.GetByIdAsync(tenantId, deviceId, configId);
        }

        public async Task<GetAllRecord<DeviceConfiguration>> GetAllAsync(int tenantId, int deviceId)
        {
            return await _repository.GetAllAsync(tenantId, deviceId);
        }

    }
}
