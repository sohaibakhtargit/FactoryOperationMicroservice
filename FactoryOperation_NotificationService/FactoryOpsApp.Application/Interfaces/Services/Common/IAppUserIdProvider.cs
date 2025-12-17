using Microsoft.AspNetCore.SignalR;

namespace FactoryOperation_NotificationService.FactoryOpsApp.Application.Interfaces.Services.Common
{
    public interface IAppUserIdProvider
    {
        string GetUserId(HubConnectionContext connection);
    }
}
