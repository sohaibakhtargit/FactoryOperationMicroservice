using FactoryOperation_KafkaMqttService.FactoryOpsApp.Application.Interfaces.Services.EventTrace;
using FactoryOperation_KafkaMqttService.FactoryOpsApp.Application.Models;
using FactoryOperation_KafkaMqttService.FactoryOpsApp.Infrastructure.Implementations.Services.Queue;

namespace FactoryOperation_KafkaMqttService.FactoryOpsApp.Infrastructure.Implementations.Services.EventTrace
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
