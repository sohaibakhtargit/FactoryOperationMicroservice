namespace FactoryOperation_WorkOrder.FactoryOpsApp.Infrastructure.Jobs
{
    using FactoryOperation_WorkOrder.FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.Common;
    using Quartz;

    [DisallowConcurrentExecution] // prevents duplicate execution
    public class WorkOrderSchedulerJob : IJob
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<WorkOrderSchedulerJob> _logger;

        public WorkOrderSchedulerJob(
            IServiceScopeFactory scopeFactory,
            ILogger<WorkOrderSchedulerJob> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            _logger.LogInformation("Quartz Job Started at {time}", DateTime.UtcNow);

            try
            {
                using var scope = _scopeFactory.CreateScope();

                var service = scope.ServiceProvider
                    .GetRequiredService<IWorkOrderSchedulerService>();

                await service.ProcessWorkOrdersAsync();

                _logger.LogInformation("Quartz Job Completed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Quartz Job Failed");
                throw;
            }
        }
    }
}
