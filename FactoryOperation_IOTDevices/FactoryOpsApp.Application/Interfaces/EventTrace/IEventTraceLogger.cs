using FactoryOperation_IOTDevices.FactoryOpsApp.Application.Models;

namespace FactoryOperation_IOTDevices.FactoryOpsApp.Application.Interfaces.EventTrace
{
    public interface IEventTraceLogger
    {
        Task TrackAsync(EventTraceEntry entry);
    }
}