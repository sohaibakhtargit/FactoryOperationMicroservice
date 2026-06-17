using System.Diagnostics.Metrics;

namespace FactoryOps.Shared.Observability
{
    public static class AppMetrics
    {
        public static readonly Meter Meter = new("FactoryOps.Messaging");

        public static readonly Counter<int> RequestCounter =
            Meter.CreateCounter<int>("iot_requests_total");
    }
}
