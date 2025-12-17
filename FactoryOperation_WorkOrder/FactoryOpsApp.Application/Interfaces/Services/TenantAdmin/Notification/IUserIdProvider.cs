using Microsoft.AspNetCore.SignalR;

namespace FactoryOperation_WorkOrder.FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.Notification
{
    public interface IUserIdProviders
    {
        public string GetUserId(HubConnectionContext connection);
    }
}
