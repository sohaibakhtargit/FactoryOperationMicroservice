using FactoryOperation_KafkaMqttService.FactoryOpsApp.Messaging.Interfaces;
using FactoryOperation_KafkaMqttService.FactoryOpsApp.Shared.Models;

namespace FactoryOperation_KafkaMqttService.FactoryOpsApp.Messaging.Services
{
    public sealed class DefaultTenantResolver : ITenantResolver
    {
        public int? ResolveFromMqtt(string topic)
        {
            // Pattern: tenant/{id}/devices/{code}/...
            var parts = topic.Split('/', StringSplitOptions.RemoveEmptyEntries);

            for (int i = 0; i < parts.Length - 1; i++)
            {
                if (parts[i].Equals("tenant", StringComparison.OrdinalIgnoreCase) &&
                    int.TryParse(parts[i + 1], out var id))
                {
                    return id;
                }
            }

            return null;
        }

        public int? ResolveFromKafka(KafkaMessageEnvelope envelope)
        {
            if (envelope.Headers == null)
                return null;

            if (envelope.Headers.TryGetValue("tenant-id", out var tenant) &&
                int.TryParse(tenant, out var id))
            {
                return id;
            }

            return null;
        }
    }
}
