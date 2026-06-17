using FactoryOperation_IOTDevices.FactoryOpsApp.Application.Interfaces.EventTrace;
using FactoryOperation_IOTDevices.FactoryOpsApp.Application.Models;
using FactoryOperation_IOTDevices.FactoryOpsApp.Infrastructure.Implementation.Queue;

namespace FactoryOperation_IOTDevices.FactoryOpsApp.Infrastructure.Implementation.EventTrace
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
