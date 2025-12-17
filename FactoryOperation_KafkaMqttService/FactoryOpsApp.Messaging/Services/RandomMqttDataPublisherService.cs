using FactoryOperation_KafkaMqttService.FactoryOpsApp.Infrastructure.DBContext;
using FactoryOperation_KafkaMqttService.FactoryOpsApp.Messaging.Config;
using FactoryOperation_KafkaMqttService.FactoryOpsApp.Messaging.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace FactoryOperation_KafkaMqttService.FactoryOpsApp.Messaging.Services
{
    // Simulator publishes random JSON to registered topics from DB across configured tenants.
    // It will try to substitute a device code into a '+' wildcard when possible via device-topic mappings.
    public class RandomMqttDataPublisherService : BackgroundService
    {
        private readonly ILogger<RandomMqttDataPublisherService> _logger;
        private readonly IMqttClientService _mqtt;
        private readonly MqttSimulatorSettings _settings;
        private readonly IServiceScopeFactory _scopeFactory;

        public RandomMqttDataPublisherService(
            IMqttClientService mqtt,
            IOptions<MqttSimulatorSettings> options,
            IServiceScopeFactory scopeFactory,
            ILogger<RandomMqttDataPublisherService> logger)
        {
            _mqtt = mqtt;
            _settings = options.Value ?? new MqttSimulatorSettings();
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (!_settings.Enabled)
            {
                _logger.LogInformation("MQTT simulator disabled.");
                return;
            }

            if ((_settings.TenantIds?.Length ?? 0) == 0)
            {
                _logger.LogWarning("Simulator: no tenant IDs configured. Set Messaging:Simulator:TenantIds");
                return;
            }

            await _mqtt.ConnectAsync(stoppingToken);
            _logger.LogInformation("Starting DB-driven MQTT simulator for tenants [{Tenants}] every {Ms} ms",
                string.Join(",", _settings.TenantIds!), _settings.IntervalMs);

            var rnd = new Random();

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var dbFactory = scope.ServiceProvider.GetRequiredService<TenantDbContextFactory>();

                    foreach (var tenantId in _settings.TenantIds!)
                    {
                        await using var db = dbFactory.GetTenantDbContext(tenantId);

                        var topics = await db.FactoryMqttTopics
                            .AsNoTracking()
                            .Where(t => t.TenantId == tenantId && t.IsActive && !t.IsDeleted && t.MqttPath != null && t.MqttPath != "")
                            .Select(t => new { t.TopicId, t.MqttPath })
                            .ToListAsync(stoppingToken);

                        var deviceLinks = await db.FactoryDeviceTopics
                            .AsNoTracking()
                            .Where(dt => dt.TenantId == tenantId && dt.IsActive && !dt.IsDeleted)
                            .Select(dt => new { dt.TopicId, dt.DeviceId })
                            .ToListAsync(stoppingToken);

                        var devices = await db.FactoryDevices
                            .AsNoTracking()
                            .Where(d => d.TenantId == tenantId && d.IsActive && !d.IsDeleted)
                            .Select(d => new { d.DeviceId, d.DeviceCode })
                            .ToListAsync(stoppingToken);

                        foreach (var top in topics)
                        {
                            var links = deviceLinks.Where(x => x.TopicId == top.TopicId).ToList();
                            var codes = links.Select(l => devices.FirstOrDefault(d => d.DeviceId == l.DeviceId)?.DeviceCode)
                                             .Where(c => !string.IsNullOrEmpty(c))
                                             .Cast<string>()
                                             .ToList();

                            string concreteTopic = top.MqttPath!;

                            // If there is a '+' wildcard, try substituting a device code
                            if (top.MqttPath!.Contains("+") && codes.Count > 0)
                            {
                                var segments = top.MqttPath.Split('/', StringSplitOptions.RemoveEmptyEntries).ToArray();
                                var idx = Array.FindIndex(segments, s => s == "+");
                                if (idx >= 0)
                                {
                                    var pick = codes[rnd.Next(codes.Count)];
                                    segments[idx] = pick;
                                    concreteTopic = string.Join('/', segments);
                                }
                            }
                            else if (top.MqttPath!.Contains("#"))
                            {
                                // Append a suffix to make it concrete
                                concreteTopic = top.MqttPath.Replace("#", $"sim/{Guid.NewGuid():N}");
                            }

                            var payload = new
                            {
                                ts = DateTime.UtcNow,
                                temperature = Math.Round(rnd.NextDouble() * 40.0 + 5.0, 2),
                                humidity = Math.Round(rnd.NextDouble() * 60.0 + 20.0, 2),
                                vibration = Math.Round(rnd.NextDouble() * 10.0, 2),
                                ok = rnd.Next(0, 100) > 5
                            };
                            var bytes = JsonSerializer.SerializeToUtf8Bytes(payload);
                            await _mqtt.PublishAsync(concreteTopic, bytes, qos: 0, retain: false, ct: stoppingToken);
                        }
                    }

                    await Task.Delay(_settings.IntervalMs, stoppingToken);
                }
                catch (OperationCanceledException) { }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Simulator loop error; continuing");
                }
            }
        }
    }
}