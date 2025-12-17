using Microsoft.AspNetCore.SignalR;

namespace FactoryOperation_AccessManagementService.FactoryOpsApp.Infrastructure.Implementation.Service.TenantAdmin.Notification
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
