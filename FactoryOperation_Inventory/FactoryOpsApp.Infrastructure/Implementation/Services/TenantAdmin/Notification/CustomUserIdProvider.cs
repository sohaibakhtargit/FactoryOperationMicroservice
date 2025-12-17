using Microsoft.AspNetCore.SignalR;
using FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.Notification;

namespace FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.Notification
{
    public class CustomUserIdProvider : IUserIdProvider
    {
        public string GetUserId(HubConnectionContext connection)
        {
            return connection.User?.FindFirst("UserId")?.Value
                ?? connection.User?.FindFirst("AdminId")?.Value
                ?? connection.User?.FindFirst("sub")?.Value;
        }
    }
}
