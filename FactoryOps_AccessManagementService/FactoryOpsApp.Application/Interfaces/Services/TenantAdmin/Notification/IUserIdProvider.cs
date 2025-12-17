using Microsoft.AspNetCore.SignalR;

namespace FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.Notification
{
    public interface IUserIdProvider
    {
        public string GetUserId(HubConnectionContext connection);
    }
}
