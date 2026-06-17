namespace FactoryOperation_IOTDevices.FactoryOpsApp.Application.Models
{
    public sealed class KafkaSettings
    {
        public string BootstrapServers { get; set; } = "44.211.113.36:29092";
		public string TopicPattern { get; set; } = @"^factoryops\.\d+\.devices\.[^.]+\..+$";
		public string ConsumerTopic { get; set; } = "factory-commands";
        public string GroupId { get; set; } = "factoryops-mqtt.v1";
        public bool EnableAutoOffsetStore { get; set; } = true;
        public bool EnableAutoCommit { get; set; } = true;
        public string DefaultKeyStrategy { get; set; } = "topic-deviceid";
        public bool EnableSSL { get; set; } = false;

        // NEW: publish to tenant-specific Kafka topics (tenant-{id}-telemetry)
        public bool UsePerTenantTopic { get; set; } = false;

        public SecuritySettings? Security { get; set; }
        public IDictionary<string, object>? ProducerConfig { get; set; }
        public IDictionary<string, object>? ConsumerConfig { get; set; }

        public SchemaRegistrySettings? SchemaRegistry { get; set; }
        public DlqSettings? Dlq { get; set; }
        public ObservabilitySettings? Observability { get; set; }
        public OperationalSettings? Operational { get; set; }
    }

    public record SecuritySettings
    {
        public string Protocol { get; init; } = "SASL_SSL"; // SSL | SASL_SSL
        public string SaslMechanism { get; init; } = "SCRAM-SHA-256";
        public string? SaslUsername { get; init; }
        public string? SaslPassword { get; init; }
        public string? SslCAPath { get; init; }
        public string? SslCertificatePath { get; init; }
        public string? SslKeyPath { get; init; }
    }

    public record SchemaRegistrySettings
    {
        public bool Enabled { get; init; } = false;
        public string? Url { get; init; }
        public string? AuthType { get; init; } // Basic | Bearer | None
        public string? Username { get; init; }
        public string? Password { get; init; }
    }

    public record DlqSettings
    {
        public bool Enabled { get; init; } = true;
        public string Topic { get; init; } = "factory-telemetry-dlq";
        public int MaxRetries { get; init; } = 5;
        public int RetryBackoffMs { get; init; } = 500;
        public bool AddHeaders { get; init; } = true;
    }

    public record ObservabilitySettings
    {
        public bool EnableJmx { get; init; } = true;
        public PrometheusSettings? Prometheus { get; init; }
        public bool EnableTracing { get; init; } = true;
        public string TracingServiceName { get; init; } = "factoryops-kafka-client";
    }

    public record PrometheusSettings
    {
        public bool Enabled { get; init; } = true;
        public int Port { get; init; } = 9180;
        public string MetricsPrefix { get; init; } = "kafka_client";
    }

    public record OperationalSettings
    {
        public int GracefulShutdownMs { get; init; } = 10_000;
        public string HealthCheckTopic { get; init; } = "__consumer_offsets";
        public CanarySettings? Canary { get; init; }
        public bool ConfigValidationAtStartup { get; init; } = true;
    }

    public record CanarySettings
    {
        public bool Enabled { get; init; } = true;
        public string TestTopic { get; init; } = "factoryops-canary";
        public int IntervalSeconds { get; init; } = 60;
    }
}
