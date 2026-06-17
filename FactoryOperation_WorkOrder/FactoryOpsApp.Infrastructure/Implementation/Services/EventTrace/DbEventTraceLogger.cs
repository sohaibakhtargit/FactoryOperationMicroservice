using FactoryOperation_WorkOrder.FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.EventTrace;
using FactoryOperation_WorkOrder.FactoryOpsApp.Application.Models;
using FactoryOperation_WorkOrder.FactoryOpsApp.Infrastructure.Implementation.Queue;

namespace FactoryOperation_WorkOrder.FactoryOpsApp.Infrastructure.Implementation.Services.EventTrace
{
    public sealed class DbEventTraceLogger : IEventTraceLogger
    {
        private readonly EventTraceQueue _queue;

        public DbEventTraceLogger(EventTraceQueue queue)
        {
            _queue = queue;
        }

        public async Task TrackAsync(EventTraceEntry e)
        {
            await _queue.WriteAsync(e);
        }
    }
}
