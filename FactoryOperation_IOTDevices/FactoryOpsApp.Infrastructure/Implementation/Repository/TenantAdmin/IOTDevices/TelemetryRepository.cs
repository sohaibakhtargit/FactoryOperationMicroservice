using FactoryOperation_IOTDevices.FactoryOpsApp.Application.Common;
using FactoryOperation_IOTDevices.FactoryOpsApp.Application.Interfaces.Repositories.TenantAdmin.IOTDevices;
using FactoryOpsApp.Application.DTOs;
using FactoryOpsApp.Domain.Entities.FactoryOpsTenants;
using FactoryOpsApp.Infrastructure.DBContext;
using Microsoft.EntityFrameworkCore;
using static FactoryOperation_IOTDevices.FactoryOpsApp.Common.CommonConstant;

namespace FactoryOperation_IOTDevices.FactoryOpsApp.Infrastructure.Implementation.Repository.TenantAdmin.IOTDevices
{
    public class TelemetryRepository : ITelemetryRepository
    {
        private readonly TenantDbContextFactory _tenantDbContext;

        public TelemetryRepository(TenantDbContextFactory tenantDbContext)
        {
            _tenantDbContext = tenantDbContext;
        }
        public async Task<CommonResponseModel> AddTelemetryAsync(Telemetry telemetry, int tenantId)
        {
            try
            {
                using var tenantDb = _tenantDbContext.GetTenantDbContext(tenantId);
                await tenantDb.Telemetry.AddAsync(telemetry);
                await tenantDb.SaveChangesAsync();

                return new CommonResponseModel
                {
                    StatusCode = StatusCode.Success,
                    StatusMessage = TelemetryStatusMessage.AddSuccess
                };
            }
            catch (Exception ex)
            {
                return new CommonResponseModel
                {
                    StatusCode = StatusCode.Error,
                    StatusMessage = $"{TelemetryStatusMessage.AddFailed}: {ex.Message}"
                };
            }
        }
        public async Task<GetAllRecord<TelemetryResponseDto>> GetTelemetryByDeviceIdAsync(int deviceId, int tenantId)
        {
            var response = new GetAllRecord<TelemetryResponseDto>();
            try
            {
                using var tenantDb = _tenantDbContext.GetTenantDbContext(tenantId);

                var device = await tenantDb.FactoryDevices
                    .Where(d => d.DeviceId == deviceId)
                    .Select(d => new DeviceTelemetryRequestDto
                    {
                        DeviceId = d.DeviceId,
                        DeviceName = d.DeviceName,
                        DeviceCode = d.DeviceCode,
                        Status = d.Status.ToString()
                    })
                    .FirstOrDefaultAsync();

                if (device == null)
                {
                    response.StatusCode = StatusCode.NotFound;
                    response.StatusMessage = TelemetryStatusMessage.DeviceNotFound;
                    response.GetAllData = null;
                    return response;
                }

                // Fetch telemetry records
                var telemetryRecords = await tenantDb.Telemetry
                    .Where(t => t.DeviceId == deviceId && t.IsActive && !t.IsDeleted)
                    .OrderByDescending(t => t.Timestamp)
                    .Select(t => new TelemetryDto
                    {
                        TelemetryId = t.TelemetryId,
                        DeviceId = t.DeviceId,
                        Timestamp = t.Timestamp,
                        SensorDataJson = t.SensorDataJson
                    })
                    .ToListAsync();

                // Fetch latest device status logs
                var statusLogs = await tenantDb.DeviceStatusLogs
                    .Where(l => l.DeviceId == deviceId && l.IsActive && !l.IsDeleted)
                    .OrderByDescending(l => l.EventTime)
                    .ToListAsync();

                var result = new TelemetryResponseDto
                {
                    Device = device,
                    TelemetryRecords = telemetryRecords,
                    StatusLogs = statusLogs
                };

                response.GetAllData = new List<TelemetryResponseDto> { result };
                response.StatusCode = StatusCode.Success;
                response.StatusMessage = TelemetryStatusMessage.FetchTelemetrySuccess;
            }
            catch (Exception ex)
            {
                response.StatusCode = StatusCode.Error;
                response.StatusMessage = $"{TelemetryStatusMessage.FetchTelemetryFailed}: {ex.Message}";
                response.GetAllData = null;
            }

            return response;
        }
        public async Task<GetAllRecord<DeviceStatusLog>> GetDeviceStatusLogsAsync(int deviceId, int tenantId, int limit = 10)
        {
            var response = new GetAllRecord<DeviceStatusLog>();

            try
            {
                using var tenantDb = _tenantDbContext.GetTenantDbContext(tenantId);

                var data = await tenantDb.DeviceStatusLogs
                    .Where(l => l.DeviceId == deviceId && l.IsActive && !l.IsDeleted)
                    .OrderByDescending(l => l.EventTime)
                    .Take(limit)
                    .ToListAsync();

                response.GetAllData = data;
                response.StatusCode = StatusCode.Success;
                response.StatusMessage = TelemetryStatusMessage.FetchStatusLogsSuccess;
            }
            catch (Exception ex)
            {
                response.StatusCode = StatusCode.Error;
                response.StatusMessage = $"{TelemetryStatusMessage.FetchStatusLogsFailed}: {ex.Message}";
            }

            return response;
        }

    }
}
