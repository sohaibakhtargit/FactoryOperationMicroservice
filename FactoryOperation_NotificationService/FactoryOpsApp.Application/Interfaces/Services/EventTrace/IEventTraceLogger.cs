using FactoryOperation_NotificationService.FactoryOpsApp.Application.Models;

namespace FactoryOperation_NotificationService.FactoryOpsApp.Application.Interfaces.Services.EventTrace
{
    public interface IEventTraceLogger
    {
        Task TrackAsync(EventTraceEntry entry);
    }
}
