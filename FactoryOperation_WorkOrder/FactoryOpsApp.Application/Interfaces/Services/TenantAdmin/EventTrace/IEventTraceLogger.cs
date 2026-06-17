using FactoryOperation_WorkOrder.FactoryOpsApp.Application.Models;

namespace FactoryOperation_WorkOrder.FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.EventTrace
{
    public interface IEventTraceLogger
    {
        Task TrackAsync(EventTraceEntry entry);
    }
}
