using FactoryOperation_IOTDevices.FactoryOpsApp.Application.Common;
using FactoryOperation_IOTDevices.FactoryOpsApp.Application.Interfaces.Repositories.TenantAdmin.IOTDevices;
using FactoryOpsApp.Application.DTOs;
using FactoryOpsApp.Domain.Entities.FactoryOpsTenants;
using FactoryOpsApp.Infrastructure.DBContext;
using Microsoft.EntityFrameworkCore;
using static FactoryOperation_IOTDevices.FactoryOpsApp.Common.CommonConstant;

namespace FactoryOperation_IOTDevices.FactoryOpsApp.Infrastructure.Implementation.Repository.TenantAdmin.IOTDevices
{
    public class DeviceConfigurationRepository : IDeviceConfigurationRepository
    {
        private readonly TenantDbContextFactory _tenantDbContextFactory;

        public DeviceConfigurationRepository(TenantDbContextFactory tenantDbContextFactory)
        {
            _tenantDbContextFactory = tenantDbContextFactory;
        }

        public async Task<DeviceConfigurationDto> AddAsync(DeviceConfigurationDto dto)
        {
            var entity = new DeviceConfiguration
            {
                TenantId = dto.TenantId,
                DeviceId = dto.DeviceId,
                SamplingRate = dto.SamplingRate,
                FirmwareVersion = dto.FirmwareVersion,
                DataFormat = dto.DataFormat,
                Protocol = dto.Protocol,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            using var tenantDb = _tenantDbContextFactory.GetTenantDbContext(dto.TenantId);
            tenantDb.DeviceConfiguration.Add(entity);
            await tenantDb.SaveChangesAsync();

            dto.ConfigId = entity.ConfigId;
            return dto;
        }

        public async Task<DeviceConfigurationDto> UpdateAsync(DeviceConfigurationDto dto)
        {
            using var tenantDb = _tenantDbContextFactory.GetTenantDbContext(dto.TenantId);

            var entity = await tenantDb.DeviceConfiguration
                .FirstOrDefaultAsync(c => c.ConfigId == dto.ConfigId && c.TenantId == dto.TenantId);

            if (entity == null) return null;

            entity.DeviceId = dto.DeviceId;
            entity.SamplingRate = dto.SamplingRate;
            entity.FirmwareVersion = dto.FirmwareVersion;
            entity.DataFormat = dto.DataFormat;
            entity.Protocol = dto.Protocol;
            entity.UpdatedAt = DateTime.UtcNow;

            await tenantDb.SaveChangesAsync();

            return dto;
        }

        public async Task<DeviceConfiguration?> GetByIdAsync(int tenantId, int deviceId, int configId)
        {
            using var tenantDb = _tenantDbContextFactory.GetTenantDbContext(tenantId);

            var entity = await tenantDb.DeviceConfiguration
                .FirstOrDefaultAsync(c => c.ConfigId == configId && c.TenantId == tenantId && c.DeviceId == deviceId);

            return entity;
        }

        public async Task<GetAllRecord<DeviceConfiguration>> GetAllAsync(int tenantId, int deviceId)
        {
            var response = new GetAllRecord<DeviceConfiguration>();

            try
            {
                using var tenantDb = _tenantDbContextFactory.GetTenantDbContext(tenantId);

                var data = await tenantDb.DeviceConfiguration
                    .Where(c => c.TenantId == tenantId && c.DeviceId == deviceId && c.IsActive && !c.IsDeleted)
                    .OrderByDescending(c => c.UpdatedAt)
                    .ToListAsync();

                response.GetAllData = data;
                response.StatusCode = StatusCode.Success;
                response.StatusMessage = DeviceConfigurationStatusMessage.FetchAllSuccess;
            }
            catch (Exception ex)
            {
                response.StatusCode = StatusCode.Error;
                response.StatusMessage = $"{DeviceConfigurationStatusMessage.FetchAllFailed}: {ex.Message}";
            }

            return response;
        }

    }


}
