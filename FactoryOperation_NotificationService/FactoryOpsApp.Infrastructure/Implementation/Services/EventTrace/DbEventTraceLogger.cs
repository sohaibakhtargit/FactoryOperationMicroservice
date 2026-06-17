using FactoryOperation_NotificationService.FactoryOpsApp.Application.Interfaces.Services.EventTrace;
using FactoryOperation_NotificationService.FactoryOpsApp.Application.Models;
using  FactoryOperation_NotificationService.FactoryOpsApp.Infrastructure.Implementation.Queue;
namespace FactoryOperation_NotificationService.FactoryOpsApp.Infrastructure.Implementation.Services.EventTrace
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
