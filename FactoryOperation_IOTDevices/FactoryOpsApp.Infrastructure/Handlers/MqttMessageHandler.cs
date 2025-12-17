using FactoryOperation_IOTDevices.FactoryOpsApp.Application.Interfaces.Handlers;
using FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.IOTDevices;
using FactoryOpsApp.Domain.Entities.FactoryOpsTenants;
using FactoryOpsApp.Infrastructure.DBContext;
using FactoryOpsApp_IOTDevices.FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.IOTDevices;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace FactoryOperation_IOTDevices.FactoryOpsApp.Infrastructure.Handlers
{
    public class MqttMessageHandler : IMqttMessageHandler
    {
        private readonly ILogger<MqttMessageHandler> _logger;
        private readonly IServiceScopeFactory _scopeFactory;

        public MqttMessageHandler(ILogger<MqttMessageHandler> logger,
                                  IServiceScopeFactory scopeFactory)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
        }

        public async Task HandleMessageAsync(MqttMessageReceivedEventArgs e)
        {
            try
            {
                _logger.LogInformation($"Processing MQTT message: {e.Topic}");

                if (e.Topic.Contains("/status"))
                {
                    await HandleDeviceStatusMessageAsync(e.Topic, e.Payload);
                }
                else if (e.Topic.Contains("/sensor/data"))
                {
                    await HandleSensorDataMessageAsync(e.Topic, e.Payload);
                }
                else if (e.Topic.Contains("/alerts"))
                {
                    await HandleAlertMessageAsync(e.Topic, e.Payload);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error handling MQTT message on topic {e.Topic}");
            }
        }

        public async Task HandleDeviceStatusMessageAsync(string topic, string payload)
        {
            try
            {
                var statusData = JsonSerializer.Deserialize<DeviceStatusMessage>(payload);
                if (statusData != null)
                {
                    // Extract device code from topic: factory/{deviceCode}/status
                    var deviceCode = topic.Split('/')[1];

                    // Update device status in database
                    // You'll need to implement this method in your repository
                    await UpdateDeviceStatusAsync(deviceCode, statusData.NewStatus);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling device status message");
            }
        }


        public async Task HandleSensorDataMessageAsync(string topic, string payload)
        {
            try
            {
                // Deserialize incoming payload
                var sensorData = JsonSerializer.Deserialize<DeviceSimulationData>(payload);

                if (sensorData == null)
                {
                    _logger.LogWarning($"Received null or invalid sensor data. Topic: {topic}");
                    return;
                }

                // Log the received sensor data details
                _logger.LogInformation($"=== SENSOR DATA RECEIVED ===");
                _logger.LogInformation($"Topic: {topic}");
                _logger.LogInformation($"DeviceId: {sensorData.DeviceId}");
                _logger.LogInformation($"DeviceName: {sensorData.DeviceName}");
                _logger.LogInformation($"TenantId: {sensorData.TenantId}");
                _logger.LogInformation($"DeviceCode: {sensorData.DeviceCode}");
                _logger.LogInformation($"Status: {sensorData.Status}");
                _logger.LogInformation($"Timestamp: {sensorData.Timestamp}");

                // Log all sensor data values
                if (sensorData.SensorData != null && sensorData.SensorData.Any())
                {
                    _logger.LogInformation($"Sensor Data Metrics ({sensorData.SensorData.Count}):");
                    foreach (var sensor in sensorData.SensorData)
                    {
                        _logger.LogInformation($"  - {sensor.Key}: {sensor.Value}");
                    }
                }
                else
                {
                    _logger.LogInformation("No sensor data metrics found");
                }
                _logger.LogInformation($"=============================");

                // Create a scope to get tenant DB and services
                using var scope = _scopeFactory.CreateScope();
                var tenantDb = scope.ServiceProvider
                                    .GetRequiredService<TenantDbContextFactory>()
                                    .GetTenantDbContext(sensorData.TenantId);

                const int MaxRecords = 10;

                // --- Maintain max telemetry records ---
                var oldTelemetry = await tenantDb.Telemetry
                    .Where(t => t.DeviceId == sensorData.DeviceId)
                    .OrderByDescending(t => t.Timestamp)
                    .Skip(MaxRecords)
                    .ToListAsync();

                if (oldTelemetry.Any())
                {
                    _logger.LogInformation($"Removing {oldTelemetry.Count} old telemetry records");
                    tenantDb.Telemetry.RemoveRange(oldTelemetry);
                }

                var telemetry = new Telemetry
                {
                    TenantId = sensorData.TenantId,
                    DeviceId = sensorData.DeviceId,
                    Timestamp = sensorData.Timestamp,
                    SensorDataJson = JsonSerializer.Serialize(sensorData.SensorData)
                };
                tenantDb.Telemetry.Add(telemetry);

                // --- Maintain max DeviceStatusLog entries ---
                var deviceStatusService = scope.ServiceProvider.GetRequiredService<IDeviceStatusService>();
                string eventType = sensorData.Status switch
                {
                    DeviceStatusEnum.Online => "Connected",
                    DeviceStatusEnum.Offline => "Disconnected",
                    DeviceStatusEnum.Error => "Error",
                    DeviceStatusEnum.Maintenance => "Maintenance",
                    DeviceStatusEnum.ConfigurationUpdated => "Configuration Updated",
                    _ => "Unknown"
                };

                string remarks = $"Telemetry received with {sensorData.SensorData?.Count ?? 0} metrics";

                // Log new status
                await deviceStatusService.LogDeviceEventAsync(sensorData.TenantId, sensorData.DeviceId, eventType, remarks);

                // Now remove older DeviceStatusLogs if more than MaxRecords exist
                var oldStatusLogs = tenantDb.DeviceStatusLogs
                    .Where(l => l.DeviceId == sensorData.DeviceId)
                    .OrderByDescending(l => l.EventTime)
                    .Skip(MaxRecords)
                    .ToList();

                if (oldStatusLogs.Any())
                {
                    _logger.LogInformation($"Removing {oldStatusLogs.Count} old status log records");
                    tenantDb.DeviceStatusLogs.RemoveRange(oldStatusLogs);
                }

                // Save all changes
                await tenantDb.SaveChangesAsync();

                _logger.LogInformation($"Telemetry and DeviceStatusLog saved for DeviceId: {sensorData.DeviceId}, TenantId: {sensorData.TenantId}, maintaining max {MaxRecords} records each");
            }
            catch (JsonException jsonEx)
            {
                _logger.LogError(jsonEx, $"JSON deserialization error. Payload: {payload}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Unexpected error handling sensor data message. Topic: {topic}");
            }
        }


        public async Task HandleAlertMessageAsync(string topic, string payload)
        {
            try
            {
                var alertData = JsonSerializer.Deserialize<DeviceAlertMessage>(payload);
                if (alertData != null)
                {
                    _logger.LogWarning($"Device alert: {alertData.Message}");

                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling alert message");
            }
        }

        private async Task UpdateDeviceStatusAsync(string deviceCode, string status)
        {
            // Implement this method to update device status in database
            // You'll need to add this method to your repository
        }
    }

    public class DeviceStatusMessage
    {
        public int DeviceId { get; set; }
        public string DeviceName { get; set; }
        public string OldStatus { get; set; }
        public string NewStatus { get; set; }
        public DateTime Timestamp { get; set; }
    }

    public class DeviceAlertMessage
    {
        public int DeviceId { get; set; }
        public string DeviceName { get; set; }
        public string AlertType { get; set; }
        public string Message { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
