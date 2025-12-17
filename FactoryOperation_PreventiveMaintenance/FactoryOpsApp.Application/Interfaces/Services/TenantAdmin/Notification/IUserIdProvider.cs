using Microsoft.AspNetCore.SignalR;

namespace FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.Notification
{
    public interface IUserIdProvider
    {
        public string GetUserId(HubConnectionContext connection);
    }
}
