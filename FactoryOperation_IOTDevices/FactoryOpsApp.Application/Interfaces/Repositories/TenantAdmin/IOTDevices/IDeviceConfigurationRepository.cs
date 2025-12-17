using FactoryOperation_IOTDevices.FactoryOpsApp.Application.Common;
using FactoryOpsApp.Application.DTOs;
using FactoryOpsApp.Domain.Entities.FactoryOpsTenants;

namespace FactoryOperation_IOTDevices.FactoryOpsApp.Application.Interfaces.Repositories.TenantAdmin.IOTDevices
{
    public interface IDeviceConfigurationRepository
    {

        Task<DeviceConfigurationDto> AddAsync(DeviceConfigurationDto model);
        Task<DeviceConfigurationDto> UpdateAsync(DeviceConfigurationDto model);
        Task<DeviceConfiguration?> GetByIdAsync(int tenantId, int deviceId, int configId);
        Task<GetAllRecord<DeviceConfiguration>> GetAllAsync(int tenantId, int deviceId);



    }
}
