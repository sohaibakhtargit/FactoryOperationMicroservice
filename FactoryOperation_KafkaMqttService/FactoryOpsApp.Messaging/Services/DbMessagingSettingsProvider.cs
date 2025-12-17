using FactoryOperation_KafkaMqttService.FactoryOpsApp.Infrastructure.DBContext;
using FactoryOperation_KafkaMqttService.FactoryOpsApp.Messaging.Config;
using FactoryOperation_KafkaMqttService.FactoryOpsApp.Messaging.Interfaces;
using FactoryOperation_KafkaMqttService.FactoryOpsApp.Messaging.Models;
using FactoryOperation_KafkaMqttService.FactoryOpsApp.Shared.Config;
using FactoryOpsApp.Domain.Entities.FactoryOpsTenants;
using FactoryOpsApp.Domain.Entities.MasterTenantsAdmin;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace FactoryOperation_KafkaMqttService.FactoryOpsApp.Messaging.Services
{
    public class DbMessagingSettingsProvider : IMessagingSettingsProvider
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

        public MessagingEffectiveSettings GetEffectiveSettings(int? tenantId = null)
        {
            var key = tenantId.HasValue ? $"msg:settings:{tenantId.Value}" : "msg:settings:global";
            if (_cache.TryGetValue(key, out MessagingEffectiveSettings cached))
                return cached;

            var effMqtt = Clone(_fallbackMqtt.Value);
            var effKafka = Clone(_fallbackKafka.Value);

            using var scope = _scopeFactory.CreateScope();
            var masterDb = scope.ServiceProvider.GetRequiredService<MasterFactoryOpsDbContext>();
            var tenantFactory = scope.ServiceProvider.GetRequiredService<TenantDbContextFactory>();

            // 1) Merge Master DB global Kafka settings (active, not deleted; latest updated)
            var g = masterDb.MessagingGlobalSettings.AsNoTracking()
                .Where(x => x.IsActive && !x.IsDeleted)
                .OrderByDescending(x => x.UpdatedAt ?? x.CreatedAt)
                .FirstOrDefault();

            if (g != null)
                effKafka = MergeKafka(effKafka, g);

            // 2) Merge per-tenant overrides (by TenantId)
            if (tenantId.HasValue && tenantId.Value > 0)
            {
                using var tenantDb = tenantFactory.GetTenantDbContext(tenantId.Value);
                var t = tenantDb.MessagingTenantSettings.AsNoTracking()
                    .Where(x => x.TenantId == tenantId.Value)
                    .OrderByDescending(x => x.UpdatedAt)
                    .FirstOrDefault();

                if (t != null)
                {
                    effMqtt = MergeMqtt(effMqtt, t);
                    effKafka = MergeKafka(effKafka, t);
                }
            }

            var eff = new MessagingEffectiveSettings
            {
                TenantId = tenantId ?? 0,
                Mqtt = effMqtt,
                Kafka = effKafka,
                LoadedAtUtc = DateTime.UtcNow,
                Source = "db-cache"
            };

            _cache.Set(key, eff, TimeSpan.FromMinutes(2));
            return eff;
        }

        public Task ReloadAsync(int? tenantId = null, CancellationToken ct = default)
        {
            var key = tenantId.HasValue ? $"msg:settings:{tenantId.Value}" : "msg:settings:global";
            _cache.Remove(key);
            SettingsChanged?.Invoke(this, tenantId);
            return Task.CompletedTask;
        }

        private static MqttSettings MergeMqtt(MqttSettings baseCfg, MessagingTenantSettings t)
        {
            var c = Clone(baseCfg);
            if (!string.IsNullOrWhiteSpace(t.MqttBrokerUrl)) c.BrokerUrl = t.MqttBrokerUrl;
            if (t.MqttBrokerPort.HasValue) c.BrokerPort = t.MqttBrokerPort.Value;
            if (!string.IsNullOrWhiteSpace(t.MqttClientId)) c.ClientId = t.MqttClientId;
            if (t.CleanSession.HasValue) c.CleanSession = t.CleanSession.Value;
            if (t.KeepAlive.HasValue) c.KeepAlive = t.KeepAlive.Value;
            if (!string.IsNullOrEmpty(t.Username)) c.Username = t.Username;
            if (!string.IsNullOrEmpty(t.Password)) c.Password = t.Password; // consider encryption
            if (!string.IsNullOrWhiteSpace(t.Topic)) c.Topic = t.Topic;
            if (t.QoS.HasValue) c.QoS = t.QoS.Value;

            if (t.OfflineBufferEnable.HasValue || t.OfflineMax.HasValue)
            {
                c.OfflineBuffer ??= new OfflineBufferSettings();
                c.OfflineBuffer = c.OfflineBuffer with
                {
                    Enable = t.OfflineBufferEnable ?? c.OfflineBuffer.Enable,
                    MaxMessages = t.OfflineMax ?? c.OfflineBuffer.MaxMessages
                };
            }
            return c;
        }

        private static KafkaSettings MergeKafka(KafkaSettings baseCfg, MessagingGlobalSettings g)
        {
            var c = Clone(baseCfg);
            if (!string.IsNullOrWhiteSpace(g.BootstrapServers)) c.BootstrapServers = g.BootstrapServers;
            if (!string.IsNullOrWhiteSpace(g.Topic)) c.Topic = g.Topic;
            if (g.UsePerTenantTopic.HasValue) c.UsePerTenantTopic = g.UsePerTenantTopic.Value;
            if (!string.IsNullOrWhiteSpace(g.DlqTopic))
                c.Dlq = c.Dlq is not null ? c.Dlq with { Topic = g.DlqTopic } : new DlqSettings { Topic = g.DlqTopic };
            if (g.EnableSSL.HasValue) c.EnableSSL = g.EnableSSL.Value;

            if (!string.IsNullOrWhiteSpace(g.ProducerConfigJson))
            {
                try
                {
                    var dict = JsonSerializer.Deserialize<Dictionary<string, object>>(g.ProducerConfigJson);
                    if (dict != null) c.ProducerConfig = dict;
                }
                catch { }
            }

            return c;
        }

        private static KafkaSettings MergeKafka(KafkaSettings baseCfg, MessagingTenantSettings t)
        {
            var c = Clone(baseCfg);
            if (!string.IsNullOrWhiteSpace(t.KafkaTopicOverride)) c.Topic = t.KafkaTopicOverride;
            if (t.UsePerTenantTopicOverride.HasValue) c.UsePerTenantTopic = t.UsePerTenantTopicOverride.Value;
            return c;
        }

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
            ProducerConfig = s.ProducerConfig != null ? new Dictionary<string, object>(s.ProducerConfig) : null,
            ConsumerConfig = s.ConsumerConfig != null ? new Dictionary<string, object>(s.ConsumerConfig) : null,
            SchemaRegistry = s.SchemaRegistry,
            Dlq = s.Dlq,
            Observability = s.Observability,
            Operational = s.Operational
        };
    }
}
