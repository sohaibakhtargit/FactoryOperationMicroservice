using FactoryOperation_KafkaMqttService.FactoryOpsApp.Application.Models;

namespace FactoryOperation_KafkaMqttService.FactoryOpsApp.Application.Interfaces.Services.EventTrace
{
    public interface IEventTraceLogger
    {
        Task TrackAsync(EventTraceEntry entry);
    }

}
