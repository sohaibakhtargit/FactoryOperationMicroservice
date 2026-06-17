using FactoryOpsApp.Domain.Entities.FactoryOpsTenants;
using FactoryOpsApp.Infrastructure.DBContext;
using FactoryOpsApp_IOTDevices.FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.IOTDevices;

namespace FactoryOperation_IOTDevices.FactoryOpsApp.Infrastructure.Implementation.Services.TenantAdmin.IOTDevices
{
    public class DeviceStatusService : IDeviceStatusService
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public DeviceStatusService(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        public async Task LogDeviceEventAsync(int tenantId, int deviceId, string eventType, string remarks)
        {
            using var scope = _scopeFactory.CreateScope();
            var tenantDb = scope.ServiceProvider
                                .GetRequiredService<TenantDbContextFactory>()
                                .GetTenantDbContext(tenantId);

            var log = new DeviceStatusLog
            {
                TenantId = tenantId,
                DeviceId = deviceId,
                EventType = eventType,
                EventTime = DateTime.UtcNow,
                Remarks = remarks
            };

            tenantDb.DeviceStatusLogs.Add(log);
            await tenantDb.SaveChangesAsync();
        }
    }

}
