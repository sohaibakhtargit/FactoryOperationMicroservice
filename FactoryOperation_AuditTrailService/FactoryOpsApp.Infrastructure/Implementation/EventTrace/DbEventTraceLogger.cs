using FactoryOperation_AuditTrailService.FactoryOpsApp.Application.Interfaces.Services.EventTrace;
using FactoryOperation_AuditTrailService.FactoryOpsApp.Application.Models;
using FactoryOperation_AuditTrailService.FactoryOpsApp.Infrastructure.Implementation.Queue;

namespace FactoryOperation_AuditTrailService.FactoryOpsApp.Infrastructure.Implementation.EventTrace
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
