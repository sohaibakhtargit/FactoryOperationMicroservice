using FactoryOperation_IOTDevices.FactoryOpsApp.Application.Interfaces.Handlers;
using FactoryOperation_IOTDevices.FactoryOpsApp.Application.Models;
using FactoryOperation_IOTDevices.FactoryOpsApp.Domain.Entities.FactoryOpsTenants;
using FactoryOpsApp.Domain.Entities.FactoryOpsTenants;
using FactoryOpsApp.Infrastructure.DBContext;
using FactoryOpsApp_IOTDevices.FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.IOTDevices;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Text.Json;

namespace FactoryOperation_IOTDevices.FactoryOpsApp.Infrastructure.Handlers
{
    public class KafkaTelemetryHandler : IKafkaTelemetryHandler
    {
        private readonly ILogger<KafkaTelemetryHandler> _logger;
        private readonly IServiceScopeFactory _scopeFactory;

        public KafkaTelemetryHandler(
            ILogger<KafkaTelemetryHandler> logger,
            IServiceScopeFactory scopeFactory)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
        }

        public async Task HandleTelemetryAsync(KafkaMessageEnvelope envelope)
        {
            try
            {
                var messageId = string.IsNullOrWhiteSpace(envelope.MessageId)
                    ? Guid.NewGuid().ToString("N")
                    : envelope.MessageId;

                _logger.LogInformation(
                    "Processing MessageId={MessageId} Key={Key}",
                    messageId,
                    envelope.Key);

                string? payloadBase64 = null;
                int tenantId = envelope.TenantId ?? 0;
                string? deviceCode = null;
                int deviceId = 0;

                // -----------------------------
                // Extract values from payload
                // -----------------------------
                if (envelope.Payload is JsonElement json)
                {
                    if (json.TryGetProperty("payloadBase64", out var payloadElement))
                        payloadBase64 = payloadElement.GetString();

                    if (tenantId == 0 && json.TryGetProperty("tenantId", out var tenantElement))
                        tenantId = tenantElement.GetInt32();

                    if (json.TryGetProperty("deviceCode", out var deviceElement))
                        deviceCode = deviceElement.GetString();
                }

                if (string.IsNullOrWhiteSpace(payloadBase64))
                {
                    _logger.LogWarning("Missing payload in Kafka message");
                    return;
                }

                if (string.IsNullOrWhiteSpace(deviceCode))
                {
                    _logger.LogWarning("DeviceCode missing in Kafka message");
                    return;
                }

                if (tenantId <= 0)
                {
                    _logger.LogWarning("Invalid TenantId in message");
                    return;
                }

                _logger.LogInformation(
                    "Telemetry incoming Tenant={TenantId} DeviceCode={DeviceCode}",
                    tenantId,
                    deviceCode);

                // -----------------------------
                // Create DB Context (FIXED)
                // -----------------------------
                using var scope = _scopeFactory.CreateScope();
                var tenantDb = scope.ServiceProvider
                    .GetRequiredService<TenantDbContextFactory>()
                    .GetTenantDbContext(tenantId); // ✅ FIXED

                // -----------------------------
                // Idempotency Check
                // -----------------------------
                var alreadyProcessed = await tenantDb.ProcessedMessages
                    .AnyAsync(x => x.MessageId == messageId);

                if (alreadyProcessed)
                {
                    _logger.LogWarning("Duplicate message skipped: {MessageId}", messageId);
                    return;
                }

                // -----------------------------
                // Decode Base64 payload
                // -----------------------------
                var decodedJson = Encoding.UTF8.GetString(Convert.FromBase64String(payloadBase64));

                _logger.LogInformation("Decoded telemetry payload: {Payload}", decodedJson);

                using var doc = JsonDocument.Parse(decodedJson);
                var root = doc.RootElement;

                var sensorData = new DeviceSimulationData
                {
                    TenantId = tenantId,
                    DeviceId = deviceId,
                    DeviceCode = deviceCode,
                    Timestamp = root.TryGetProperty("ts", out var tsEl)
                        ? tsEl.GetDateTime()
                        : DateTime.UtcNow,
                    Status = root.TryGetProperty("ok", out var okEl) && okEl.GetBoolean()
                        ? DeviceStatusEnum.Online
                        : DeviceStatusEnum.Error,
                    SensorData = new Dictionary<string, decimal>()
                };

                // -----------------------------
                // Extract numeric telemetry
                // -----------------------------
                foreach (var prop in root.EnumerateObject())
                {
                    if (prop.Name is "ts" or "ok")
                        continue;

                    if (prop.Value.ValueKind == JsonValueKind.Number &&
                        prop.Value.TryGetDecimal(out var val))
                    {
                        sensorData.SensorData[prop.Name] = val;
                    }
                }

                // -----------------------------
                // Resolve device
                // -----------------------------
                var device = await tenantDb.FactoryDevices
                    .AsNoTracking()
                    .FirstOrDefaultAsync(d =>
                        d.DeviceCode == sensorData.DeviceCode &&
                        d.TenantId == sensorData.TenantId &&
                        !d.IsDeleted);

                if (device == null)
                {
                    _logger.LogWarning(
                        "Device not found. Tenant={TenantId}, DeviceCode={DeviceCode}",
                        sensorData.TenantId,
                        sensorData.DeviceCode);

                    return;
                }

                sensorData.DeviceId = device.DeviceId;

                const int MaxRecords = 10;

                // -----------------------------
                // TELEMETRY TABLE
                // -----------------------------
                var oldTelemetry = await tenantDb.Telemetry
                    .Where(t => t.DeviceId == sensorData.DeviceId)
                    .OrderByDescending(t => t.Timestamp)
                    .Skip(MaxRecords)
                    .ToListAsync();

                if (oldTelemetry.Any())
                    tenantDb.Telemetry.RemoveRange(oldTelemetry);

                tenantDb.Telemetry.Add(new Telemetry
                {
                    TenantId = sensorData.TenantId,
                    DeviceId = sensorData.DeviceId,
                    Timestamp = sensorData.Timestamp,
                    SensorDataJson = JsonSerializer.Serialize(sensorData.SensorData)
                });

                // -----------------------------
                // DEVICE STATUS LOG
                // -----------------------------
                var deviceStatusService =
                    scope.ServiceProvider.GetRequiredService<IDeviceStatusService>();

                string eventType = sensorData.Status switch
                {
                    DeviceStatusEnum.Online => "Connected",
                    DeviceStatusEnum.Offline => "Disconnected",
                    DeviceStatusEnum.Error => "Error",
                    DeviceStatusEnum.Maintenance => "Maintenance",
                    DeviceStatusEnum.ConfigurationUpdated => "Configuration Updated",
                    _ => "Unknown"
                };

                string remarks =
                    $"Telemetry received with {sensorData.SensorData?.Count ?? 0} metrics";

                await deviceStatusService.LogDeviceEventAsync(
                    sensorData.TenantId,
                    sensorData.DeviceId,
                    eventType,
                    remarks);

                var oldLogs = tenantDb.DeviceStatusLogs
                    .Where(l => l.DeviceId == sensorData.DeviceId)
                    .OrderByDescending(l => l.EventTime)
                    .Skip(MaxRecords)
                    .ToList();

                if (oldLogs.Any())
                    tenantDb.DeviceStatusLogs.RemoveRange(oldLogs);

                // -----------------------------
                // Mark as processed
                // -----------------------------
                tenantDb.ProcessedMessages.Add(new ProcessedMessage
                {
                    MessageId = messageId,
                    ProcessedAt = DateTime.UtcNow
                });

                // -----------------------------
                // Save (Race condition safe)
                // -----------------------------
                try
                {
                    await tenantDb.SaveChangesAsync();
                }
                catch (DbUpdateException ex)
                {
                    if (ex.InnerException?.Message.Contains("duplicate") == true ||
                        ex.InnerException?.Message.Contains("unique") == true)
                    {
                        _logger.LogWarning("Duplicate detected at DB level: {MessageId}", messageId);
                        return;
                    }

                    throw;
                }

                _logger.LogInformation(
                    "Telemetry persisted for device={Device} tenant={Tenant}",
                    sensorData.DeviceId,
                    sensorData.TenantId);

                _logger.LogInformation(
                    "Processed MessageId={MessageId} Tenant={TenantId} Device={DeviceCode}",
                    messageId,
                    tenantId,
                    deviceCode);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kafka telemetry handler failed");
            }
        }

        public Task HandleStatusAsync(KafkaMessageEnvelope envelope)
        {
            _logger.LogInformation("Status message received key={Key}", envelope.Key);
            return Task.CompletedTask;
        }

        public Task HandleAlertAsync(KafkaMessageEnvelope envelope)
        {
            _logger.LogWarning("Alert message received key={Key}", envelope.Key);
            return Task.CompletedTask;
        }
    }
}