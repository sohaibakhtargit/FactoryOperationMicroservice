using FactoryOperation_WorkOrder.FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.Notification;
using Microsoft.AspNetCore.SignalR;

namespace FactoryOperation_WorkOrder.FactoryOpsApp.Infrastructure.Implementation.Services.TenantAdmin.Notification
{
    public class CustomUserIdProvider : IUserIdProviders
    {
        public string GetUserId(HubConnectionContext connection)
        {
            return connection.User?.FindFirst("UserId")?.Value
                ?? connection.User?.FindFirst("AdminId")?.Value
                ?? connection.User?.FindFirst("sub")?.Value;
        }
    }
}
