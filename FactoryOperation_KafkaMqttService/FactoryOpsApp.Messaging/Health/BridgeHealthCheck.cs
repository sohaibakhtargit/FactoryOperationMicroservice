using FactoryOperation_KafkaMqttService.FactoryOpsApp.Messaging.Interfaces;
using FactoryOperation_KafkaMqttService.FactoryOpsApp.Shared.Interfaces;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace FactoryOperation_KafkaMqttService.FactoryOpsApp.Messaging.Health
{
    public class BridgeHealthCheck : IHealthCheck
    {
        private readonly IMqttClientService _mqtt;
        private readonly IKafkaProducerService _kafka;
        private readonly ILogger<BridgeHealthCheck> _logger;

        public static Func<int>? BacklogAccessor { get; set; }

        public BridgeHealthCheck(IMqttClientService mqtt, IKafkaProducerService kafka, ILogger<BridgeHealthCheck> logger)
        {
            _mqtt = mqtt;
            _kafka = kafka;
            _logger = logger;
        }

        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            var mqttOk = _mqtt.IsConnected;
            var backlog = BacklogAccessor?.Invoke() ?? 0;
            var healthy = mqttOk; // Kafka producer is created eagerly; errors show on produce.

            var data = new Dictionary<string, object>
            {
                ["mqttConnected"] = mqttOk,
                ["backlogDepth"] = backlog
            };

            return Task.FromResult(healthy
                ? HealthCheckResult.Healthy("Bridge OK", data)
                : HealthCheckResult.Degraded("Bridge degraded"));
        }
    }
}