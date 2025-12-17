using FactoryOperation_IOTDevices.FactoryOpsApp.Application.Interfaces.Repositories.TenantAdmin.IOTDevices;
using FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.IOTDevices;
using FactoryOpsApp.Domain.Entities.FactoryOpsTenants;
using FactoryOpsApp.Infrastructure.DBContext;
using FactoryOpsApp_IOTDevices.FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.IOTDevices;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace FactoryOperation_IOTDevices.FactoryOpsApp.Infrastructure.Implementation.Services.TenantAdmin.IOTDevices
{
    public class IoTDataSimulator : IIoTDataSimulator, IHostedService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IMqttService _mqttService;
        private readonly ILogger<IoTDataSimulator> _logger;
        private Timer _simulationTimer;

        public bool IsSimulationRunning { get; private set; }

        public IoTDataSimulator(IServiceProvider serviceProvider, IMqttService mqttService, ILogger<IoTDataSimulator> logger)
        {
            _serviceProvider = serviceProvider;
            _mqttService = mqttService;
            _logger = logger;
            IsSimulationRunning = false;
        }

        public async Task StartSimulationAsync()
        {
            if (IsSimulationRunning) return;

            IsSimulationRunning = true;
            _simulationTimer = new Timer(SimulateDevices, null, TimeSpan.Zero, TimeSpan.FromSeconds(10));
            _logger.LogInformation("IoT Data Simulation started");
        }

        public Task StopSimulationAsync()
        {
            IsSimulationRunning = false;
            _simulationTimer?.Dispose();
            _simulationTimer = null;
            _logger.LogInformation("IoT Data Simulation stopped");
            return Task.CompletedTask;
        }

        private async void SimulateDevices(object state)
        {
            using var scope = _serviceProvider.CreateScope();
            var _dbContext = scope.ServiceProvider.GetRequiredService<MasterFactoryOpsDbContext>();
            var deviceRepositoryFactory = scope.ServiceProvider.GetRequiredService<IFactoryDeviceRepository>();

            try
            {
                var tenants = await _dbContext.FactoryTenants
                    .Where(t => !t.IsDeleted && t.IsActive)
                    .ToListAsync();

                foreach (var tenant in tenants)
                {
                    try
                    {
                        var devicesResponse = await deviceRepositoryFactory.GetAllDevicesAsync(tenant.TenantId);

                        if (devicesResponse.StatusCode == "200" && devicesResponse.GetAllData != null)
                        {
                            foreach (var deviceDto in devicesResponse.GetAllData)
                            {
                                var device = new FactoryDevice
                                {
                                    DeviceId = deviceDto.DeviceId,
                                    TenantId = deviceDto.TenantId,
                                    DeviceName = deviceDto.DeviceName,
                                    DeviceCode = deviceDto.DeviceCode,
                                    Category = deviceDto.Category,
                                    Status = deviceDto.Status,
                                };

                                await SimulateDeviceDataAsync(device);
                                await SimulateDeviceStatusChangeAsync(device);
                            }
                        }
                    }
                    catch (Exception tenantEx)
                    {
                        _logger.LogError(tenantEx, $"Error simulating devices for TenantId: {tenant.TenantId}");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in device simulation across all tenants");
            }
        }

        //public async Task SimulateDeviceDataAsync(FactoryDevice device)
        //{
        //    var random = new Random();
        //    var sensorData = new Dictionary<string, decimal>();

        //    switch (device.Category)
        //    {
        //        case DeviceCategoryEnum.Environmental:
        //            sensorData["temperature"] = Convert.ToDecimal(Math.Round(20 + random.NextDouble() * 15, 2));
        //            sensorData["humidity"] = Convert.ToDecimal(Math.Round(30 + random.NextDouble() * 50, 2));
        //            sensorData["pressure"] = Convert.ToDecimal(Math.Round(1000 + random.NextDouble() * 50, 2));
        //            break;

        //        case DeviceCategoryEnum.Mechanical:
        //            sensorData["vibration"] = Convert.ToDecimal(Math.Round(random.NextDouble() * 10, 2));
        //            sensorData["rpm"] = Convert.ToDecimal(random.Next(1000, 3000));
        //            sensorData["load"] = Convert.ToDecimal(Math.Round(random.NextDouble() * 100, 2));
        //            break;

        //        case DeviceCategoryEnum.Electrical:
        //            sensorData["voltage"] = Convert.ToDecimal(Math.Round(220 + random.NextDouble() * 20, 2));
        //            sensorData["current"] = Convert.ToDecimal(Math.Round(5 + random.NextDouble() * 15, 2));
        //            sensorData["power"] = Convert.ToDecimal(Math.Round(1000 + random.NextDouble() * 2000, 2));
        //            break;

        //        default:
        //            sensorData["value"] = Convert.ToDecimal(random.Next(0, 100));
        //            break;
        //    }

        //    var simulationData = new DeviceSimulationData
        //    {
        //        DeviceId = device.DeviceId,
        //        DeviceName = device.DeviceName,
        //        DeviceCode = device.DeviceCode,
        //        Status = device.Status,
        //        TenantId = device.TenantId,
        //        SensorData = sensorData,
        //        Timestamp = DateTime.UtcNow
        //    };

        //    var topic = $"factory/{device.DeviceCode}/sensor/data";
        //    var payload = JsonSerializer.Serialize(simulationData);

        //    await _mqttService.PublishAsync(topic, payload);
        //}

        public async Task SimulateDeviceDataAsync(FactoryDevice device)
        {
            var random = new Random();
            var sensorData = new Dictionary<string, decimal>
            {
                ["temperature"] = Convert.ToDecimal(Math.Round(20 + random.NextDouble() * 15, 2)),
                ["vibration"] = Convert.ToDecimal(Math.Round(random.NextDouble() * 10, 2)),
                ["pressure"] = Convert.ToDecimal(Math.Round(1000 + random.NextDouble() * 50, 2))
            };

            var simulationData = new DeviceSimulationData
            {
                DeviceId = device.DeviceId,
                DeviceName = device.DeviceName,
                DeviceCode = device.DeviceCode,
                Status = device.Status,
                TenantId = device.TenantId,
                SensorData = sensorData,
                Timestamp = DateTime.UtcNow
            };

            var topic = $"factory/{device.DeviceCode}/sensor/data";
            var payload = JsonSerializer.Serialize(simulationData);

            await _mqttService.PublishAsync(topic, payload);
        }


        public async Task SimulateDeviceStatusChangeAsync(FactoryDevice device)
        {
            var random = new Random();

            // 10% chance to change status
            if (random.Next(0, 100) < 10)
            {
                var statuses = Enum.GetValues(typeof(DeviceStatusEnum));
                var newStatus = (DeviceStatusEnum)statuses.GetValue(random.Next(statuses.Length));

                var statusData = new
                {

                    device.DeviceId,
                    device.DeviceName,
                    OldStatus = device.Status.ToString(),
                    NewStatus = newStatus.ToString(),
                    Timestamp = DateTime.UtcNow
                };

                var topic = $"factory/{device.DeviceCode}/status";
                var payload = JsonSerializer.Serialize(statusData);

                await _mqttService.PublishAsync(topic, payload);
            }
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await StartSimulationAsync();
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await StopSimulationAsync();
        }
    }
}