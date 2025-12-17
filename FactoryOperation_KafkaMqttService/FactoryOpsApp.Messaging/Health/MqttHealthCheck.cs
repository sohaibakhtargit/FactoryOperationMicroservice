using FactoryOperation_KafkaMqttService.FactoryOpsApp.Messaging.Interfaces;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace FactoryOperation_KafkaMqttService.FactoryOpsApp.Messaging.Health
{
    public sealed class MqttHealthCheck : IHealthCheck
    {
        private readonly IMqttClientService _mqtt;

        public MqttHealthCheck(IMqttClientService mqtt)
        {
            _mqtt = mqtt;
        }

        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            // Minimal: mark healthy when connected; degraded otherwise.
            if (_mqtt is null)
                return Task.FromResult(HealthCheckResult.Unhealthy("MQTT service not available"));

            return Task.FromResult(_mqtt.IsConnected
                ? HealthCheckResult.Healthy("MQTT connected")
                : HealthCheckResult.Degraded("MQTT not connected"));
        }
    }
}
