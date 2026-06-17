using Confluent.Kafka;
using FactoryOperation_AuditTrailService.FactoryOpsApp.Application.Interfaces.Services.EventTrace;
using FactoryOperation_AuditTrailService.FactoryOpsApp.Application.Interfaces.Services.Shared;
using FactoryOperation_AuditTrailService.FactoryOpsApp.Application.Models;
using FactoryOpsApp.Domain.Entities.MasterTenantsAdmin;
using FactoryOpsApp.Infrastructure.DBContext;
using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

public sealed class KafkaAuditConsumer : BackgroundService
{
    private readonly ILogger<KafkaAuditConsumer> _logger;
    private readonly KafkaSettings _settings;
    private readonly IServiceScopeFactory _scopeFactory;

    private const string DlqSuffix = "-dlq";

    public KafkaAuditConsumer(
        ILogger<KafkaAuditConsumer> logger,
        IOptions<KafkaSettings> settings,
        IServiceScopeFactory scopeFactory
        )
    {
        _logger = logger;
        _settings = settings.Value;
        _scopeFactory = scopeFactory;
    }

    // ================= SSL SUPPORT (ADDED ONLY) =================
    private void ApplySecurityConfig(ClientConfig config)
    {
        if (!_settings.EnableSSL)
        {
            config.SecurityProtocol = SecurityProtocol.Plaintext;
            return;
        }

        var sec = _settings.Security;
        if (sec == null) return;

        if (!string.IsNullOrWhiteSpace(sec.Protocol))
            config.SecurityProtocol = Enum.Parse<SecurityProtocol>(sec.Protocol, true);

        if (!string.IsNullOrWhiteSpace(sec.SaslMechanism))
            config.SaslMechanism = Enum.Parse<SaslMechanism>(sec.SaslMechanism, true);

        config.SaslUsername = sec.SaslUsername;
        config.SaslPassword = sec.SaslPassword;

        if (!string.IsNullOrEmpty(sec.SslCAPath))
            config.SslCaLocation = sec.SslCAPath;

        if (!string.IsNullOrEmpty(sec.SslCertificatePath))
            config.SslCertificateLocation = sec.SslCertificatePath;

        if (!string.IsNullOrEmpty(sec.SslKeyPath))
            config.SslKeyLocation = sec.SslKeyPath;
    }

    // ================= MAIN LOOP =================
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("KafkaAuditConsumer starting...");

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
                _logger.LogCritical(ex, "Audit consumer crashed. Restarting in 10 seconds...");
                await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
            }
        }

        _logger.LogInformation("KafkaAuditConsumer stopped.");
    }

    // ================= CONSUMER CORE =================
    private async Task StartConsumer(CancellationToken stoppingToken)
    {
        var regex = new Regex(_settings.TopicPattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);

        var config = new ConsumerConfig
        {
            BootstrapServers = _settings.BootstrapServers,
            GroupId = _settings.GroupId,

            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = false,
            EnableAutoOffsetStore = false,

            ClientId = $"factoryops-audit-{Environment.MachineName}",
            SocketKeepaliveEnable = true,

            FetchMinBytes = 50_000,
            FetchWaitMaxMs = 500,
            MaxPartitionFetchBytes = 1_048_576,
            FetchMaxBytes = 5_242_880,

            MaxPollIntervalMs = 300_000,
            SessionTimeoutMs = 30_000,
            HeartbeatIntervalMs = 10_000
        };

        ApplySecurityConfig(config); // SSL applied

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
            _logger.LogWarning("No topics matched pattern. Retrying in 10s...");
            await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
            return;
        }

        consumer.Subscribe(matchedTopics);

        while (!stoppingToken.IsCancellationRequested)
        {
            ConsumeResult<string, byte[]>? cr = null;

            try
            {
                cr = consumer.Consume(stoppingToken);
                if (cr == null)
                    continue;

                if (!regex.IsMatch(cr.Topic))
                    continue;

                var envelope = TryReadEnvelope(cr.Message.Value);
                using var scope = _scopeFactory.CreateScope();
                var _trace = scope.ServiceProvider.GetRequiredService<IEventTraceLogger>();
                await _trace.TrackAsync(new EventTraceEntry
                {
                    CorrelationId = envelope?.CorrelationId,
                    TenantId = envelope?.TenantId,
                    Service = "KafkaAuditConsumer",
                    Stage = "CONSUME_RECEIVED",
                    Topic = cr.Topic,
                    Partition = cr.Partition.Value,
                    Offset = cr.Offset.Value,
                    Status = "SUCCESS",
                    Message = "Kafka message received"
                });

                _logger.LogDebug(
                    "Consumed topic={Topic} p={Partition} o={Offset} size={Size}",
                    cr.Topic,
                    cr.Partition,
                    cr.Offset,
                    cr.Message.Value?.Length ?? 0);

                var rawBytes = cr.Message.Value ?? Array.Empty<byte>();

                await _trace.TrackAsync(new EventTraceEntry
                {
                    CorrelationId = envelope?.CorrelationId,
                    TenantId = envelope?.TenantId,
                    Service = "KafkaAuditConsumer",
                    Stage = "PROCESSING_STARTED",
                    Topic = cr.Topic,
                    Partition = cr.Partition.Value,
                    Offset = cr.Offset.Value,
                    Status = "SUCCESS",
                    Message = "Audit processing started"
                });

                var resolvedPayload = await ResolvePayloadAsync(rawBytes, scope, stoppingToken);

                await ProcessAuditAsync(cr.Topic, resolvedPayload, scope, stoppingToken);

                consumer.StoreOffset(cr);
                consumer.Commit(cr);

                await _trace.TrackAsync(new EventTraceEntry
                {
                    CorrelationId = envelope?.CorrelationId,
                    TenantId = envelope?.TenantId,
                    Service = "KafkaAuditConsumer",
                    Stage = "PROCESSED",
                    Topic = cr.Topic,
                    Partition = cr.Partition.Value,
                    Offset = cr.Offset.Value,
                    Status = "SUCCESS",
                    Message = "Audit stored successfully"
                });

                _logger.LogInformation(
                    "Audit message processed: {Topic} Offset={Offset}",
                    cr.Topic,
                    cr.Offset);
            }
            catch (ConsumeException ex)
            {
                _logger.LogError(ex,
                    "Kafka consume error Topic={Topic} Partition={Partition} Offset={Offset}",
                    ex.ConsumerRecord?.Topic,
                    ex.ConsumerRecord?.Partition,
                    ex.ConsumerRecord?.Offset);

                if (cr != null)
                {
                    await TraceFailure(cr, ex);
                    await SendToDlqAsync(consumer, cr, ex, stoppingToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kafka audit worker unexpected error");

                if (cr != null)
                {
                    await TraceFailure(cr, ex);
                    await SendToDlqAsync(consumer, cr, ex, stoppingToken);
                }
            }
        }

        _logger.LogInformation("Closing Kafka audit consumer...");
        consumer.Close();
    }

    // ================= AUDIT PROCESSOR =================
    private async Task ProcessAuditAsync(
        string topic,
        byte[] payloadBytes,
        IServiceScope scope,
        CancellationToken ct)
    {
        var json = Encoding.UTF8.GetString(payloadBytes);

        DomainEvent<JsonElement>? evt;

        try
        {
            evt = JsonSerializer.Deserialize<DomainEvent<JsonElement>>(
                json,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

            if (evt == null)
            {
                _logger.LogWarning("Invalid audit event. Topic={Topic}", topic);
                return;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to deserialize audit event. Raw={Raw}", json);
            return;
        }

        var db = scope.ServiceProvider
            .GetRequiredService<MasterFactoryOpsDbContext>();

        var audit = new Audit_Log_MasterDb
        {
            EventType = evt.EventType,
            TenantId = evt.TenantId,
            Timestamp = evt.OccurredAtUtc ?? DateTime.UtcNow,
            Action = evt.EventType,
            UserName = Environment.UserName,
            Ipaddress = evt.IpAddress,
            Details = evt.SourceService + " " + evt.EventType,
            IsActive = true,
            IsDeleted = false
        };

        db.Audit_Log_MasterDb.Add(audit);
        await db.SaveChangesAsync(ct);

        _logger.LogInformation(
            "Audit stored: {EventType} Tenant={TenantId} Correlation={CorrelationId}",
            evt.EventType,
            evt.TenantId,
            evt.CorrelationId);
    }

    // ================= DLQ =================
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

            var producerConfig = new ProducerConfig
            {
                BootstrapServers = _settings.BootstrapServers
            };

            ApplySecurityConfig(producerConfig);

            using var producer = new ProducerBuilder<string, byte[]>(producerConfig).Build();

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
                Service = "KafkaAuditConsumer",
                Stage = "DLQ_ROUTED",
                Topic = dlqTopic,
                Partition = res.Partition.Value,
                Offset = res.Offset.Value,
                Status = "WARNING",
                Message = "Audit message routed to DLQ"
            });

            _logger.LogWarning(
                "Poison audit message sent to DLQ {DlqTopic} (offset {Offset})",
                dlqTopic,
                cr.Offset.Value);
        }
        catch (Exception e)
        {
            _logger.LogCritical(e, "Failed to route audit poison message to DLQ");
        }
    }

    // ================= HELPERS =================
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
            Service = "KafkaAuditConsumer",
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
            var json = Encoding.UTF8.GetString(bytes);
            return JsonSerializer.Deserialize<KafkaMessageEnvelope>(json);
        }
        catch
        {
            return null;
        }
    }

    // ================= PAYLOAD RESOLUTION =================
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

    // ================= TOPIC DISCOVERY =================
    private Task<List<string>> GetMatchingTopicsAsync(
        string bootstrapServers,
        Regex regex,
        ILogger logger)
    {
        var adminConfig = new AdminClientConfig
        {
            BootstrapServers = bootstrapServers
        };

        ApplySecurityConfig(adminConfig);

        using var admin = new AdminClientBuilder(adminConfig).Build();

        var metadata = admin.GetMetadata(TimeSpan.FromSeconds(10));

        var topics = metadata.Topics
            .Where(t => !t.Error.IsError && regex.IsMatch(t.Topic))
            .Select(t => t.Topic)
            .ToList();

        logger.LogInformation("Audit matched {Count} topics: {Topics}",
            topics.Count,
            string.Join(", ", topics));

        return Task.FromResult(topics);
    }
}
