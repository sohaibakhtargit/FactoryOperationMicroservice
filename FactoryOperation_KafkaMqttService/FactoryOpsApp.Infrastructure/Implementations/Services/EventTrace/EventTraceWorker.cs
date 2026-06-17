using FactoryOperation_KafkaMqttService.FactoryOpsApp.Application.Models;
using FactoryOperation_KafkaMqttService.FactoryOpsApp.Domain.Entities.FactoryOpsTenants;
using FactoryOperation_KafkaMqttService.FactoryOpsApp.Infrastructure.DBContext;
using FactoryOperation_KafkaMqttService.FactoryOpsApp.Infrastructure.Implementations.Services.Queue;

namespace FactoryOperation_KafkaMqttService.FactoryOpsApp.Infrastructure.Implementations.Services.EventTrace
{
    public class EventTraceWorker : BackgroundService
    {
        private readonly EventTraceQueue _queue;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<EventTraceWorker> _logger;

        public EventTraceWorker(
            EventTraceQueue queue,
            IServiceScopeFactory scopeFactory,
            ILogger<EventTraceWorker> logger)
        {
            _queue = queue;
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var batch = new List<EventTraceEntry>(100);

            await foreach (var entry in _queue.ReadAllAsync(stoppingToken))
            {
                batch.Add(entry);

                if (batch.Count >= 100)
                {
                    await SaveBatch(batch);
                    batch.Clear();
                }
            }
        }

        private async Task SaveBatch(List<EventTraceEntry> batch)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();

                var tenantFactory = scope.ServiceProvider
                    .GetRequiredService<TenantDbContextFactory>();

                // Group traces per tenant
                var grouped = batch.GroupBy(x => x.TenantId ?? 0);

                foreach (var group in grouped)
                {
                    var tenantId = group.Key;

                    using var db = tenantFactory.GetTenantDbContext(tenantId);

                    var traces = group.Select(Map).ToList();

                    await db.FactoryEventTraces.AddRangeAsync(traces);
                    await db.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save event traces");
            }
        }

        private static FactoryEventTrace Map(EventTraceEntry e)
        {
            return new FactoryEventTrace
            {
                CorrelationId = e.CorrelationId,
                TenantId = e.TenantId,
                Service = e.Service,
                Stage = e.Stage,
                Topic = e.Topic,
                Partition = e.Partition,
                Offset = e.Offset,
                Status = e.Status,
                Message = e.Message,
                Error = e.Error,
                CreatedAt = DateTime.UtcNow,
                IsActive = true,
                IsDeleted = false
            };
        }
    }
}
