namespace FactoryOperation_KafkaMqttService.FactoryOpsApp.Shared.Models
{
    /// <summary>
    /// Standard event envelope used across FactoryOps event-driven platform.
    /// This envelope is protocol-agnostic and supports bidirectional bridges.
    /// </summary>
    public sealed class KafkaMessageEnvelope
    {
        // ─────────────────────────────
        // Kafka-level metadata
        // ─────────────────────────────

        /// <summary>
        /// Kafka message key (used for partitioning & ordering).
        /// </summary>
        public string Key { get; init; } = string.Empty;
        public string MessageId { get; init; } = Guid.NewGuid().ToString("N"); 
        /// <summary>
        /// Event type (e.g., DeviceTelemetryReceived, WorkOrderCreated).
        /// </summary>
        public string? EventType { get; init; } = string.Empty;
        public string? IpAddress { get; init; } = string.Empty;

        /// <summary>
        /// Event schema version (e.g., v1, v2).
        /// </summary>
        public string EventVersion { get; init; } = "v1";

        /// <summary>
        /// Producer service name (e.g., FactoryOpsKafkaMqttService).
        /// </summary>
        public string Producer { get; init; } = string.Empty;

        /// <summary>
        /// Source protocol or system (MQTT, Kafka, API, Scheduler).
        /// </summary>
        public string Source { get; init; } = "MQTT";
        public int? TenantId { get; init; }

        // ─────────────────────────────
        // Event timing
        // ─────────────────────────────

        /// <summary>
        /// When the event actually occurred in the source system.
        /// </summary>
        public DateTime OccurredAt { get; init; } = DateTime.UtcNow;

        /// <summary>
        /// When the event was produced into Kafka.
        /// </summary>
        public DateTime Timestamp { get; init; } = DateTime.UtcNow;

        // ─────────────────────────────
        // Payload
        // ─────────────────────────────

        /// <summary>
        /// Event payload (domain-agnostic).
        /// Serialized as JSON before publishing.
        /// </summary>
        public object Payload { get; init; } = new { };

        // ─────────────────────────────
        // Headers & routing metadata
        // ─────────────────────────────

        /// <summary>
        /// Optional headers for routing, tracing, and multi-tenant context.
        /// </summary>
        public IDictionary<string, string>? Headers { get; init; }

        // ─────────────────────────────
        // Optional correlation / tracing
        // ─────────────────────────────

        /// <summary>
        /// Correlation ID for distributed tracing.
        /// </summary>
        public string? CorrelationId { get; init; }

        /// <summary>
        /// Causation ID (which event caused this event).
        /// Useful for workflows & debugging.
        /// </summary>
        public string? CausationId { get; init; }
    }
}
