using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace FactoryOperation_KafkaMqttService.FactoryOpsApp.Messaging.Observability
{
    public static class Telemetry
    {
        public const string SourceName = "FactoryOpsApp.Messaging";

        // Traces
        public static readonly ActivitySource Activity = new(SourceName);

        // Metrics
        public static readonly Meter Meter = new(SourceName, "1.0.0");

        public static readonly Counter<long> MqttMessagesReceived = Meter.CreateCounter<long>("mqtt.messages.received");
        public static readonly Counter<long> MqttBytesReceived = Meter.CreateCounter<long>("mqtt.bytes.received");
        public static readonly Counter<long> MqttPublishes = Meter.CreateCounter<long>("mqtt.messages.published");
        public static readonly Counter<long> MqttPublishErrors = Meter.CreateCounter<long>("mqtt.publish.errors");

        public static readonly Counter<long> KafkaMessagesProduced = Meter.CreateCounter<long>("kafka.messages.produced");
        public static readonly Counter<long> KafkaProduceErrors = Meter.CreateCounter<long>("kafka.produce.errors");

        public static readonly Histogram<double> KafkaProduceLatencyMs = Meter.CreateHistogram<double>("kafka.produce.latency.ms");
        public static readonly Histogram<double> BridgeLatencyMs = Meter.CreateHistogram<double>("bridge.endtoend.latency.ms");

        // Step 4: Observable gauge for bridge backlog depth
        private static Func<int>? _bridgeDepthProvider;

        static Telemetry()
        {
            Meter.CreateObservableGauge<long>(
                name: "bridge.queue.depth",
                observeValue: () =>
                {
                    var v = (long)(_bridgeDepthProvider?.Invoke() ?? 0);
                    return new Measurement<long>(v);
                },
                unit: "messages",
                description: "Number of MQTT messages waiting in the bridge channel");
        }

        public static void RegisterBridgeQueueDepth(Func<int> provider)
        {
            _bridgeDepthProvider = provider;
        }
    }
}
