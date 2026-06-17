using Confluent.Kafka;

namespace FactoryOperation_KafkaMqttService.FactoryOpsApp.Common.CommonConstantFiles
{
    public static class KafkaClientDefaults
    {
        public const Acks ProducerAcks = Acks.All;

        public const bool EnableIdempotence = true;

        public const int LingerMs = 20;

        public const int BatchSize = 65536;

        public const CompressionType Compression = CompressionType.Lz4;

        public const int MessageTimeoutMs = 120000;

        public const int RetryBackoffMs = 100;

        public const int MaxInFlightRequests = 5;

        public const int QueueBufferingMaxKb = 524288;

        public const int MessageMaxBytes = 1000000;

        public const int Retries = int.MaxValue;

        public const bool SocketKeepAlive = true;

    }

    public static class EventTraceConstants
    {
        public static class Services
        {
            public const string KafkaProducer = "KafkaProducerService";
            public const string KafkaConsumer = "KafkaTelemetryConsumer";
            public const string KafkaBridge = "KafkaMqttBridgeService";
        }

        public static class Stages
        {
            public const string ProduceRequested = "PRODUCE_REQUESTED";
            public const string Produced = "PRODUCED";
            public const string ConsumeReceived = "CONSUME_RECEIVED";
            public const string Processed = "PROCESSED";
            public const string Failed = "FAILED";
            public const string DlqRouted = "DLQ_ROUTED";
        }

        public static class Status
        {
            public const string Success = "SUCCESS";
            public const string Error = "ERROR";
            public const string Warning = "WARNING";
        }

        public static class Messages
        {
            public const string KafkaProduceRequested = "Kafka produce requested";
            public const string KafkaProduced = "Kafka message delivered";
            public const string KafkaConsumed = "Telemetry message received";
            public const string KafkaDlqRouted = "Message routed to DLQ";
        }
    }

    public static class LogMessages
    {
        public const string KafkaProducerError =
            "Kafka producer error: {Reason}, IsFatal: {IsFatal}, Code: {Code}";

        public const string KafkaProducerDebug =
            "Kafka producer: {Message}";

        public const string KafkaProduced =
            "Produced to {Topic} key={Key} p={Partition} o={Offset}";

        public const string KafkaBufferFull =
            "Kafka buffer full (attempt {Attempt}/{Max}). Backing off {Backoff}ms";

        public const string KafkaUnexpectedError =
            "Unexpected error during Produce for topic {Topic} key {Key} (attempt {Attempt}/{Max})";

        public const string KafkaDlqWriteFailed =
            "Failed to write to DLQ on attempt {Attempt}/{Max}";

        public const string KafkaDlqFailure =
            "Could not route message to DLQ after {Attempts} attempts";
    }
}



