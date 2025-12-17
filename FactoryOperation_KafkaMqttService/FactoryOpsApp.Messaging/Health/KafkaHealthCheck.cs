using Confluent.Kafka;
using FactoryOperation_KafkaMqttService.FactoryOpsApp.Shared.Config;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

namespace FactoryOpsApp.Messaging.Health
{
    public class KafkaHealthCheck : IHealthCheck
    {
        private readonly KafkaSettings _settings;

        public KafkaHealthCheck(IOptions<KafkaSettings> settings)   
        {
            _settings = settings.Value;
        }

        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                var cfg = new AdminClientConfig { BootstrapServers = _settings.BootstrapServers };
                using var admin = new AdminClientBuilder(cfg).Build();
                var md = admin.GetMetadata(TimeSpan.FromSeconds(5));
                if (md?.Brokers != null && md.Brokers.Count > 0)
                {
                    return Task.FromResult(HealthCheckResult.Healthy("Kafka reachable"));
                }
                return Task.FromResult(HealthCheckResult.Unhealthy("Kafka metadata returned no brokers"));
            }
            catch (Exception ex)
            {
                return Task.FromResult(HealthCheckResult.Unhealthy("Kafka unreachable", ex));
            }
        }
    }
}