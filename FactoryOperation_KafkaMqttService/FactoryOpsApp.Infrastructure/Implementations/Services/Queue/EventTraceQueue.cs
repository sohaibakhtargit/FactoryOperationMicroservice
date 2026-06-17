using FactoryOperation_KafkaMqttService.FactoryOpsApp.Application.Models;
using System.Threading.Channels;

namespace FactoryOperation_KafkaMqttService.FactoryOpsApp.Infrastructure.Implementations.Services.Queue
{
    public class EventTraceQueue
    {
        private readonly Channel<EventTraceEntry> _channel;

        public EventTraceQueue()
        {
            _channel = Channel.CreateUnbounded<EventTraceEntry>(
                new UnboundedChannelOptions
                {
                    SingleReader = true,
                    SingleWriter = false
                });
        }

        public ValueTask WriteAsync(EventTraceEntry entry)
        {
            return _channel.Writer.WriteAsync(entry);
        }

        public IAsyncEnumerable<EventTraceEntry> ReadAllAsync(CancellationToken ct)
        {
            return _channel.Reader.ReadAllAsync(ct);
        }
    }
}
