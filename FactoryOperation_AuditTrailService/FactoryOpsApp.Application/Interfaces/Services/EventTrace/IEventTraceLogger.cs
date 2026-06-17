using FactoryOperation_AuditTrailService.FactoryOpsApp.Application.Models;

namespace FactoryOperation_AuditTrailService.FactoryOpsApp.Application.Interfaces.Services.EventTrace
{
    public interface IEventTraceLogger
    {
        Task TrackAsync(EventTraceEntry entry);
    }
}
