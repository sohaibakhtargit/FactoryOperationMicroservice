using FactoryOperation_NotificationService.FactoryOpsApp.Application.Interfaces.Services.Common;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace FactoryOperation_NotificationService.FactoryOpsApp.Infrastructure.Implementation.Services.Common
{
    public class AppUserIdProvider : IAppUserIdProvider
    {
        public string GetUserId(HubConnectionContext connection)
        {
            return connection.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? connection.User?.FindFirst("sub")?.Value
                ?? connection.User?.FindFirst("user_id")?.Value;
        }
    }
}
