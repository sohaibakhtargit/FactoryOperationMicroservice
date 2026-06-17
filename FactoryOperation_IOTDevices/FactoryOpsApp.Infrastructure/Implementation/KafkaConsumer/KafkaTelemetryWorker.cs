using Confluent.Kafka;
using FactoryOperation_IOTDevices.FactoryOpsApp.Application.Interfaces.EventTrace;
using FactoryOperation_IOTDevices.FactoryOpsApp.Application.Interfaces.Handlers;
using FactoryOperation_IOTDevices.FactoryOpsApp.Application.Interfaces.Shared;
using FactoryOperation_IOTDevices.FactoryOpsApp.Application.Models;
using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

public sealed class KafkaTelemetryConsumer : BackgroundService
{
    private readonly ILogger<KafkaTelemetryConsumer> _logger;
    private readonly KafkaSettings _settings;
    private readonly IServiceScopeFactory _scopeFactory;

    private const string DlqSuffix = "-dlq";

    public KafkaTelemetryConsumer(
      ILogger<KafkaTelemetryConsumer> logger,
      IOptions<KafkaSettings> settings,
      IServiceScopeFactory scopeFactory)
    {
        _logger = logger;
        _settings = settings.Value;
        _scopeFactory = scopeFactory;
    }

    // ============================
    // MAIN LOOP
    // ============================
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("KafkaTelemetryConsumer starting...");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await StartConsumer(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogCritical(
                    ex,
                    "Telemetry consumer crashed. Restarting in 10 seconds...");
                await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
            }
        }

        _logger.LogInformation("KafkaTelemetryConsumer stopped.");
    }

    // ============================
    // CONSUMER CORE
    // ============================
    private async Task StartConsumer(CancellationToken stoppingToken)
    {
        var topicPattern = _settings.TopicPattern
            ?? _settings.ConsumerTopic
            ?? throw new InvalidOperationException("Kafka consumer topic or pattern is required");

        var regex = new Regex(topicPattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);

        var config = new ConsumerConfig
        {
            BootstrapServers = _settings.BootstrapServers,
            GroupId = _settings.GroupId,

            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = false,
            EnableAutoOffsetStore = false,

            SecurityProtocol = SecurityProtocol.Plaintext,
            ClientId = $"factoryops-telemetry-{Environment.MachineName}",

            FetchMinBytes = 50_000,
            FetchWaitMaxMs = 500,
            MaxPartitionFetchBytes = 1_048_576,
            FetchMaxBytes = 5_242_880,

            MaxPollIntervalMs = 300_000,
            SessionTimeoutMs = 30_000,
            HeartbeatIntervalMs = 10_000
        };

        ApplySecurity(config, _settings);

        using var consumer = new ConsumerBuilder<string, byte[]>(config)
            .SetErrorHandler((_, e) =>
                _logger.LogError("Kafka error: {Reason}", e.Reason))
            .Build();

        var matchedTopics = await GetMatchingTopicsAsync(
            _settings.BootstrapServers,
            regex,
            _logger);

        if (!matchedTopics.Any())
        {
            _logger.LogWarning("No telemetry topics matched pattern. Retrying in 10s...");
            await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
            return;
        }

        consumer.Subscribe(matchedTopics);

        _logger.LogInformation(
            "Telemetry consumer subscribed to topics: {Topics}",
            string.Join(", ", matchedTopics));

        while (!stoppingToken.IsCancellationRequested)
        {
            ConsumeResult<string, byte[]>? cr = null;

            try
            {
                cr = consumer.Consume(stoppingToken);
                if (cr == null || !regex.IsMatch(cr.Topic))
                    continue;

                var envelope = TryReadEnvelope(cr.Message.Value);

                // ================= TRACE: RECEIVED =================
                using var scope = _scopeFactory.CreateScope();
                var _trace = scope.ServiceProvider.GetRequiredService<IEventTraceLogger>();
                await _trace.TrackAsync(new EventTraceEntry
                {
                    CorrelationId = envelope?.CorrelationId,
                    TenantId = envelope?.TenantId,
                    Service = "KafkaTelemetryConsumer",
                    Stage = "CONSUME_RECEIVED",
                    Topic = cr.Topic,
                    Partition = cr.Partition.Value,
                    Offset = cr.Offset.Value,
                    Status = "SUCCESS",
                    Message = "Telemetry message received"
                });


                // ================= PAYLOAD RESOLUTION =================
                var resolvedPayload = await ResolvePayloadAsync(
                    cr.Message.Value ?? Array.Empty<byte>(),
                    scope,
                    stoppingToken);

                // ================= HANDLER =================
                var handler = scope.ServiceProvider
                    .GetRequiredService<IKafkaTelemetryHandler>();

                var safeEnvelope = envelope ?? new KafkaMessageEnvelope
                {
                    Payload = BuildPayloadElement(resolvedPayload)
                };

                await handler.HandleTelemetryAsync(safeEnvelope);


                consumer.StoreOffset(cr);
                consumer.Commit(cr);

                // ================= TRACE: SUCCESS =================
                await _trace.TrackAsync(new EventTraceEntry
                {
                    CorrelationId = envelope?.CorrelationId,
                    TenantId = envelope?.TenantId,
                    Service = "KafkaTelemetryConsumer",
                    Stage = "PROCESSED",
                    Topic = cr.Topic,
                    Partition = cr.Partition.Value,
                    Offset = cr.Offset.Value,
                    Status = "SUCCESS",
                    Message = "Telemetry processed successfully"
                });
            }
            catch (ConsumeException ex)
            {
                _logger.LogError(ex,
                    "Kafka consume error Topic={Topic} Partition={Partition} Offset={Offset}",
                    cr?.Topic,
                    cr?.Partition,
                    cr?.Offset);

                if (cr != null)
                {
                    await TraceFailure(cr, ex);
                    await SendToDlqAsync(consumer, cr, ex, stoppingToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Telemetry consumer processing error");

                if (cr != null)
                {
                    await TraceFailure(cr, ex);
                    await SendToDlqAsync(consumer, cr, ex, stoppingToken);
                }
            }
        }

        _logger.LogInformation("Closing Kafka telemetry consumer...");
        consumer.Close();
    }

    // ============================
    // DLQ
    // ============================
    private async Task SendToDlqAsync(
        IConsumer<string, byte[]> consumer,
        ConsumeResult<string, byte[]> cr,
        Exception ex,
        CancellationToken ct)
    {
        try
        {
            var dlqTopic = $"{cr.Topic}{DlqSuffix}";
            var envelope = TryReadEnvelope(cr.Message.Value);

            using var producer = new ProducerBuilder<string, byte[]>(
                new ProducerConfig
                {
                    BootstrapServers = _settings.BootstrapServers
                }).Build();

            var headers = new Headers
            {
                { "x-error", Encoding.UTF8.GetBytes(ex.Message) },
                { "x-original-topic", Encoding.UTF8.GetBytes(cr.Topic) },
                { "x-original-partition", Encoding.UTF8.GetBytes(cr.Partition.Value.ToString()) },
                { "x-original-offset", Encoding.UTF8.GetBytes(cr.Offset.Value.ToString()) }
            };

            var res = await producer.ProduceAsync(
                dlqTopic,
                new Message<string, byte[]>
                {
                    Key = cr.Message.Key,
                    Value = cr.Message.Value ?? Array.Empty<byte>(),
                    Headers = headers
                },
                ct);

            consumer.StoreOffset(cr);
            consumer.Commit(cr);

            using var scope = _scopeFactory.CreateScope();
            var _trace = scope.ServiceProvider.GetRequiredService<IEventTraceLogger>();

            await _trace.TrackAsync(new EventTraceEntry
            {
                CorrelationId = envelope?.CorrelationId,
                TenantId = envelope?.TenantId,
                Service = "KafkaTelemetryConsumer",
                Stage = "DLQ_ROUTED",
                Topic = dlqTopic,
                Partition = res.Partition.Value,
                Offset = res.Offset.Value,
                Status = "WARNING",
                Message = "Telemetry routed to DLQ"
            });

            _logger.LogWarning(
                "Telemetry message sent to DLQ {DlqTopic} offset {Offset}",
                dlqTopic,
                cr.Offset.Value);
        }
        catch (Exception e)
        {
            _logger.LogCritical(e, "Failed to route telemetry message to DLQ");
        }
    }

    // ============================
    // HELPERS
    // ============================
    private async Task TraceFailure(
        ConsumeResult<string, byte[]> cr,
        Exception ex)
    {
        KafkaMessageEnvelope? envelope = null;

        try
        {
            envelope = TryReadEnvelope(cr.Message.Value);
        }
        catch { }

        using var scope = _scopeFactory.CreateScope();
        var _trace = scope.ServiceProvider.GetRequiredService<IEventTraceLogger>();
        await _trace.TrackAsync(new EventTraceEntry
        {
            CorrelationId = envelope?.CorrelationId,
            TenantId = envelope?.TenantId,
            Service = "KafkaTelemetryConsumer",
            Stage = "FAILED",
            Topic = cr.Topic,
            Partition = cr.Partition.Value,
            Offset = cr.Offset.Value,
            Status = "ERROR",
            Error = ex.Message
        });
    }

    private static KafkaMessageEnvelope? TryReadEnvelope(byte[]? bytes)
    {
        if (bytes == null || bytes.Length == 0)
            return null;

        try
        {
            return JsonSerializer.Deserialize<KafkaMessageEnvelope>(
                Encoding.UTF8.GetString(bytes),
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
        }
        catch
        {
            return null;
        }
    }

    // ============================
    // EXTERNAL PAYLOAD
    // ============================
    private async Task<byte[]> ResolvePayloadAsync(
        byte[] rawBytes,
        IServiceScope scope,
        CancellationToken ct)
    {
        var rawString = Encoding.UTF8.GetString(rawBytes);

        if (!rawString.Contains("\"Type\":\"EXTERNAL_PAYLOAD\""))
            return rawBytes;

        var reference = JsonSerializer.Deserialize<ExternalPayloadKafkaMessage>(rawString);
        if (reference?.Type != "EXTERNAL_PAYLOAD")
            return rawBytes;

        _logger.LogInformation("Downloading external payload from {Url}", reference.BlobUrl);

        var reader = scope.ServiceProvider
            .GetRequiredService<IExternalPayloadReader>();

        return await reader.ReadAsync(reference.BlobUrl, ct);
    }

    // ============================
    // TOPIC DISCOVERY
    // ============================
    private static Task<List<string>> GetMatchingTopicsAsync(
        string bootstrapServers,
        Regex regex,
        ILogger logger)
    {
        using var admin = new AdminClientBuilder(
            new AdminClientConfig
            {
                BootstrapServers = bootstrapServers
            }).Build();

        var metadata = admin.GetMetadata(TimeSpan.FromSeconds(10));

        var topics = metadata.Topics
            .Where(t => !t.Error.IsError && regex.IsMatch(t.Topic))
            .Select(t => t.Topic)
            .ToList();

        logger.LogInformation(
            "Telemetry matched {Count} topics: {Topics}",
            topics.Count,
            string.Join(", ", topics));

        return Task.FromResult(topics);
    }

    // ============================
    // SECURITY
    // ============================
    private static void ApplySecurity(
        ConsumerConfig config,
        KafkaSettings settings)
    {
        if (!settings.EnableSSL)
        {
            config.SecurityProtocol = SecurityProtocol.Plaintext;
            return;
        }

        var sec = settings.Security;
        if (sec == null) return;

        if (!string.IsNullOrWhiteSpace(sec.Protocol))
        {
            config.SecurityProtocol = sec.Protocol.ToUpperInvariant() switch
            {
                "SSL" => SecurityProtocol.Ssl,
                "SASL_SSL" => SecurityProtocol.SaslSsl,
                "SASL_PLAINTEXT" => SecurityProtocol.SaslPlaintext,
                "PLAINTEXT" => SecurityProtocol.Plaintext,
                _ => config.SecurityProtocol
            };
        }

        if (!string.IsNullOrWhiteSpace(sec.SaslMechanism))
        {
            config.SaslMechanism = sec.SaslMechanism.ToUpperInvariant() switch
            {
                "PLAIN" => SaslMechanism.Plain,
                "SCRAM-SHA-256" => SaslMechanism.ScramSha256,
                "SCRAM-SHA-512" => SaslMechanism.ScramSha512,
                "OAUTHBEARER" => SaslMechanism.OAuthBearer,
                _ => config.SaslMechanism
            };
        }

        if (!string.IsNullOrEmpty(sec.SaslUsername))
        {
            config.SaslUsername = sec.SaslUsername;
            config.SaslPassword = sec.SaslPassword;
        }

        if (!string.IsNullOrEmpty(sec.SslCAPath) &&
            File.Exists(sec.SslCAPath))
        {
            config.SslCaLocation = sec.SslCAPath;
        }
    }
    private static JsonElement BuildPayloadElement(byte[] bytes)
    {
        var json = JsonSerializer.Serialize(new
        {
            payloadBase64 = Convert.ToBase64String(bytes)
        });

        using var doc = JsonDocument.Parse(json);
        return doc.RootElement.Clone();
    }

}
