namespace FactoryOperation_KafkaMqttService.FactoryOpsApp.Messaging.Config
{
    public class MqttSettings
    {
        public string BrokerUrl { get; set; } = "test.mosquitto.org";
        public int BrokerPort { get; set; } = 1883;
        public string ClientId { get; set; } = "factory-ops-app-test-12348203479237492639-3938er9fdh";
        public string? Username { get; set; }
        public string? Password { get; set; }
        public string Topic { get; set; } = "tenant/+/devices/+/telemetry";
        public int QoS { get; set; } = 0;
        public bool CleanSession { get; set; } = false;
        public int KeepAlive { get; set; } = 60;
        public int ReconnectDelaySeconds { get; set; } = 5;

        public LwtSettings? Lwt { get; set; }
        public OfflineBufferSettings? OfflineBuffer { get; set; }
        public TlsSettings? Tls { get; set; }

        public PublishSettings? Publish { get; set; }
        public SubscribeSettings? Subscribe { get; set; }

        public ObservabilitySettings? Observability { get; set; }
    }

    public record LwtSettings
    {
        public string Topic { get; init; } = "factory/devices/status";
        public string Payload { get; init; } = "{\"status\":\"offline\"}";
        public int Qos { get; init; } = 1;
        public bool Retain { get; init; } = true;
    }

    public record OfflineBufferSettings
    {
        public bool Enable { get; init; } = true;
        public int MaxMessages { get; init; } = 10000;
        public string Storage { get; init; } = "inmemory"; // inmemory | localfile | sqlite
        public string? LocalFilePath { get; init; }
    }

    public record TlsSettings
    {
        public bool Enable { get; init; } = true;
        public bool UseMutualTls { get; init; } = false;
        public string? CaCertPath { get; init; }
        public string? ClientCertPath { get; init; }
        public string? ClientKeyPath { get; init; }
        public string[]? AllowedTlsVersions { get; init; } = new[] { "Tls12", "Tls13" };
    }

    public record PublishSettings
    {
        public int MaxPayloadBytes { get; init; } = 1_048_576;
        public bool ChunkLargePayloads { get; init; } = true;
        public int ChunkSizeBytes { get; init; } = 262_144;
    }

    public record SubscribeSettings
    {
        public int MaxConcurrentHandlers { get; init; } = 16;
        public int WorkerPoolSize { get; init; } = 32;
    }

    public record ObservabilitySettings
    {
        public bool EnableTracing { get; init; } = true;
        public string[] PropagationHeaders { get; init; } = new[] { "traceparent", "tracestate", "correlation-id" };
        public string MetricsPrefix { get; init; } = "mqtt";
    }

}
