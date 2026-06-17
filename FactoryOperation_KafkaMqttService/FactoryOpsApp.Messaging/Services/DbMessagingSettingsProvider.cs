using FactoryOperation_KafkaMqttService.FactoryOpsApp.Infrastructure.DBContext;
using FactoryOperation_KafkaMqttService.FactoryOpsApp.Messaging.Config;
using FactoryOperation_KafkaMqttService.FactoryOpsApp.Messaging.Interfaces;
using FactoryOperation_KafkaMqttService.FactoryOpsApp.Messaging.Models;
using FactoryOperation_KafkaMqttService.FactoryOpsApp.Shared.Config;
using FactoryOperation_KafkaMqttService.FactoryOpsApp.Domain.Entities.FactoryOpsTenants;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using System.Text.Json;
using static FactoryOperation_KafkaMqttService.FactoryOpsApp.Shared.Config.KafkaSettings;

namespace FactoryOperation_KafkaMqttService.FactoryOpsApp.Messaging.Services
{
    public sealed class DbMessagingSettingsProvider : IMessagingSettingsProvider
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IMemoryCache _cache;
        private readonly IOptions<MqttSettings> _fallbackMqtt;
        private readonly IOptions<KafkaSettings> _fallbackKafka;

        public event EventHandler<int?>? SettingsChanged;

        public DbMessagingSettingsProvider(
            IServiceScopeFactory scopeFactory,
            IMemoryCache cache,
            IOptions<MqttSettings> fallbackMqtt,
            IOptions<KafkaSettings> fallbackKafka)
        {
            _scopeFactory = scopeFactory;
            _cache = cache;
            _fallbackMqtt = fallbackMqtt;
            _fallbackKafka = fallbackKafka;
        }

        // ======================================================
        // PUBLIC API
        // ======================================================
        public MessagingEffectiveSettings GetEffectiveSettings(int? tenantId = null)
        {
            var cacheKey = tenantId.HasValue && tenantId > 0
       ? $"msg:settings:tenant:{tenantId.Value}"
       : "msg:settings:global";

            // SAFE cache read
            if (_cache.TryGetValue(cacheKey, out var cachedObj) &&
                cachedObj is MessagingEffectiveSettings cached)
            {
                return cached;
            }

            // --------------------------------------------------
            // Start from appsettings fallback (SAFE + NON-NULL)
            // --------------------------------------------------
            var fallbackMqtt = _fallbackMqtt.Value
                ?? throw new InvalidOperationException("Fallback MQTT settings are not configured");

            var fallbackKafka = _fallbackKafka.Value
                ?? throw new InvalidOperationException("Fallback Kafka settings are not configured");

            // Start from appsettings fallback
            var effMqtt = Clone(_fallbackMqtt.Value);
            var effKafka = Clone(_fallbackKafka.Value);

            using var scope = _scopeFactory.CreateScope();
            var tenantFactory = scope.ServiceProvider.GetRequiredService<TenantDbContextFactory>();

            // IMPORTANT:
            // Even "global" config lives in tenant DB in your model
            //using var tenantDb = tenantFactory.GetTenantDbContext(tenantId ?? 0);
            var dbTenantId = tenantId.HasValue && tenantId > 0
                ? tenantId.Value
                : 0;

            using var tenantDb = tenantFactory.GetTenantDbContext(dbTenantId);
            // ======================================================
            // 1) KAFKA CONFIG
            // Priority:
            //   1) Tenant specific
            //   2) Global (TenantId = null)
            // ======================================================
            var kafkaCfg = tenantDb.Set<KafkaConfigurations>()
             .AsNoTracking()
             .Where(x =>
                 x.IsActive &&
                 !x.IsDeleted
             )
             .OrderByDescending(x => x.UpdatedAt)
             .FirstOrDefault();

            if (kafkaCfg != null)
                effKafka = MergeKafka(effKafka, kafkaCfg);

            // ======================================================
            // 2) MQTT CONFIG
            // Priority:
            //   1) Tenant specific
            //   2) Global
            // ======================================================
            var mqttCfg = tenantDb.Set<MqttConfigurations>()
                        .AsNoTracking()
                        .Where(x =>
                            x.IsActive &&
                            !x.IsDeleted
                        )
                        .OrderByDescending(x => x.UpdatedAt)
                        .FirstOrDefault();
            if (mqttCfg != null)
                effMqtt = MergeMqtt(effMqtt, mqttCfg);

            // ======================================================
            // 3) BRIDGE CONFIG
            // Priority:
            //   1) Tenant specific
            //   2) Global
            // Then lowest Priority value wins
            // ======================================================
            var bridgeCfg = tenantDb.Set<BridgeConfigurations>()
                     .AsNoTracking()
                     .Where(x =>
                         x.Enabled &&
                         x.IsActive &&
                         !x.IsDeleted
                     )
                     .OrderBy(x => x.Priority)  
                     .FirstOrDefault();

            if (bridgeCfg != null)
                ApplyBridge(effKafka, effMqtt, bridgeCfg);

           
            var eff = new MessagingEffectiveSettings
            {
                TenantId = tenantId ?? 0,
                Mqtt = effMqtt,
                Kafka = effKafka,
                LoadedAtUtc = DateTime.UtcNow,
                Source = "db-cache"
            };

            _cache.Set(cacheKey, eff, new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5),
                SlidingExpiration = TimeSpan.FromMinutes(2)
            });
            return eff;
        }

        public Task ReloadAsync(int? tenantId = null, CancellationToken ct = default)
        {
            var key = tenantId.HasValue && tenantId > 0
                ? $"msg:settings:tenant:{tenantId.Value}"
                : "msg:settings:global";

            _cache.Remove(key);
            SettingsChanged?.Invoke(this, tenantId);
            return Task.CompletedTask;
        }

        // ======================================================
        // MERGE: MQTT
        // ======================================================
        private static MqttSettings MergeMqtt(
            MqttSettings baseCfg,
            MqttConfigurations db)
        {
            var c = Clone(baseCfg);

            var template = !string.IsNullOrWhiteSpace(db.ClientIdTemplate)
                ? db.ClientIdTemplate
                : c.ClientIdTemplate;

            if (!string.IsNullOrWhiteSpace(template))
            {
                c.ClientId = template.Replace("{tenantId}", db.TenantId?.ToString() ?? "0");
            }

            if (string.IsNullOrWhiteSpace(c.ClientId))
            {
                c.ClientId = $"factoryops-{db.TenantId ?? 0}-{Guid.NewGuid():N}";
            }

            c.CleanSession = db.CleanSession;

            if (!string.IsNullOrEmpty(db.Username))
                c.Username = db.Username;

            if (!string.IsNullOrEmpty(db.Password))
                c.Password = db.Password;

            if (db.KeepAliveSeconds > 0)
                c.KeepAlive = db.KeepAliveSeconds;

            if (!string.IsNullOrWhiteSpace(db.TopicTemplate) &&
                db.TopicTemplate.Contains("/"))
            {
                c.Topic = db.TopicTemplate;
            }

            c.QoS = db.SubscriptionQos;

            // Offline buffer
            if (db.OfflineBuffering.RootElement.ValueKind == JsonValueKind.Object)
            {
                try
                {
                    c.OfflineBuffer =
                        JsonSerializer.Deserialize<OfflineBufferSettings>(
                            db.OfflineBuffering.RootElement.GetRawText());
                }
                catch { }
            }

            return c;
        }

        // ======================================================
        // MERGE: KAFKA
        // ======================================================
        private static KafkaSettings MergeKafka(
            KafkaSettings baseCfg,
            KafkaConfigurations db)
        {
            var c = Clone(baseCfg);

            // BootstrapServers intentionally NOT overridden
            // Reason:
            //   - Broker infra is platform-owned
            //   - Prevents tenants from hijacking cluster access

            if (!string.IsNullOrWhiteSpace(db.GroupId))
                c.GroupId = db.GroupId;

            //if (!string.IsNullOrWhiteSpace(db.TopicPattern))
            //    c.Topic = db.TopicPattern;

            c.UsePerTenantTopic = db.UsePerTenantTopic;

            // Security
            c.Security ??= new SecuritySettings();

            if (!string.IsNullOrWhiteSpace(db.SecurityProtocol))
                c.Security = c.Security with { Protocol = db.SecurityProtocol };

            if (!string.IsNullOrWhiteSpace(db.SaslMechanism))
                c.Security = c.Security with { SaslMechanism = db.SaslMechanism };

            if (!string.IsNullOrWhiteSpace(db.SaslUsername))
                c.Security = c.Security with { SaslUsername = db.SaslUsername };

            if (!string.IsNullOrWhiteSpace(db.SaslPassword))
                c.Security = c.Security with { SaslPassword = db.SaslPassword };

            // Producer config
            if (db.ProducerConfig.RootElement.ValueKind == JsonValueKind.Object)
            {
                try
                {
                    c.ProducerConfig =
                        JsonSerializer.Deserialize<Dictionary<string, object>>(
                            db.ProducerConfig.RootElement.GetRawText());
                }
                catch { }
            }

            // Consumer config
            if (db.ConsumerConfig.RootElement.ValueKind == JsonValueKind.Object)
            {
                try
                {
                    c.ConsumerConfig =
                        JsonSerializer.Deserialize<Dictionary<string, object>>(
                            db.ConsumerConfig.RootElement.GetRawText());
                }
                catch { }
            }

            // DLQ
            if (db.DlqConfig.RootElement.ValueKind == JsonValueKind.Object)
            {
                try
                {
                    c.Dlq =
                        JsonSerializer.Deserialize<DlqSettings>(
                            db.DlqConfig.RootElement.GetRawText());
                }
                catch { }
            }

            return c;
        }

        // ======================================================
        // APPLY BRIDGE RULES
        // ======================================================
        //private static void ApplyBridge(
        //    KafkaSettings kafka,
        //    MqttSettings mqtt,
        //    BridgeConfigurations bridge)
        //{
        //    if (bridge.Direction.Equals("KafkaToMqtt", StringComparison.OrdinalIgnoreCase))
        //    {
        //        kafka.EnableKafkaToMqtt = true;
        //        kafka.KafkaToMqttTopic = bridge.SourcePattern;
        //        mqtt.Topic = bridge.TargetPattern;
        //    }
        //    else
        //    {
        //        kafka.EnableKafkaToMqtt = false;
        //        mqtt.Topic = bridge.SourcePattern;
        //    }

        //    if (!string.IsNullOrWhiteSpace(bridge.DlqTopic))
        //    {
        //        kafka.Dlq = kafka.Dlq is not null
        //            ? kafka.Dlq with { Topic = bridge.DlqTopic }
        //            : new DlqSettings { Topic = bridge.DlqTopic };
        //    }
        //}

        private static void ApplyBridge(
        KafkaSettings kafka,
        MqttSettings mqtt,
        BridgeConfigurations bridge)
        {
            var direction = bridge.Direction?.ToLowerInvariant();

            switch (direction)
            {
                case "mqtttokafka":

                    mqtt.Topic = bridge.SourcePattern;
                    kafka.EnableKafkaToMqtt = false;

                    break;

                case "kafkatomqtt":

                    kafka.EnableKafkaToMqtt = true;
                    mqtt.Topic = bridge.TargetPattern;
                    kafka.KafkaToMqttTopic =
                     !string.IsNullOrWhiteSpace(kafka.KafkaToMqttTopic)
                         ? kafka.KafkaToMqttTopic
                         : @"factoryops\.\d+\.devices\..+\.telemetry";

                    break;

                case "bidirectional":

                    mqtt.Topic = bridge.SourcePattern;
                    kafka.EnableKafkaToMqtt = true;
                    kafka.KafkaToMqttTopic =
                     !string.IsNullOrWhiteSpace(kafka.KafkaToMqttTopic)
                         ? kafka.KafkaToMqttTopic
                         : @"factoryops\.\d+\.devices\..+\.telemetry";

                    break;
            }

            if (!string.IsNullOrWhiteSpace(bridge.DlqTopic))
            {
                kafka.Dlq = kafka.Dlq is not null
                    ? kafka.Dlq with { Topic = bridge.DlqTopic }
                    : new DlqSettings { Topic = bridge.DlqTopic };
            }
        }

        // ======================================================
        // CLONE HELPERS
        // ======================================================
        private static MqttSettings Clone(MqttSettings s) => new()
        {
            BrokerUrl = s.BrokerUrl,
            BrokerPort = s.BrokerPort,
            ClientId = s.ClientId,
            Username = s.Username,
            Password = s.Password,
            Topic = s.Topic,
            QoS = s.QoS,
            CleanSession = s.CleanSession,
            KeepAlive = s.KeepAlive,
            ReconnectDelaySeconds = s.ReconnectDelaySeconds,
            Lwt = s.Lwt,
            OfflineBuffer = s.OfflineBuffer,
            Tls = s.Tls,
            Publish = s.Publish,
            Subscribe = s.Subscribe,
            Observability = s.Observability
        };

        private static KafkaSettings Clone(KafkaSettings s) => new()
        {
            BootstrapServers = s.BootstrapServers,
            Topic = s.Topic,
            ConsumerTopic = s.ConsumerTopic,
            GroupId = s.GroupId,
            DefaultKeyStrategy = s.DefaultKeyStrategy,
            EnableSSL = s.EnableSSL,
            UsePerTenantTopic = s.UsePerTenantTopic,
            Security = s.Security,
            ProducerConfig = s.ProducerConfig != null
                ? new Dictionary<string, object>(s.ProducerConfig)
                : null,
            ConsumerConfig = s.ConsumerConfig != null
                ? new Dictionary<string, object>(s.ConsumerConfig)
                : null,
            SchemaRegistry = s.SchemaRegistry,
            Dlq = s.Dlq,
            Observability = s.Observability,
            Operational = s.Operational,
            EnableKafkaToMqtt = s.EnableKafkaToMqtt,
            KafkaToMqttTopic = s.KafkaToMqttTopic
        };
    }
}
