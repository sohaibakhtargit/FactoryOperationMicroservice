using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace FactoryOperation_NotificationService.FactoryOpsApp.Infrastructure.Implementation.Services.Common
{
    public class AppUserIdProvider : IUserIdProvider
    {
        public string? GetUserId(HubConnectionContext connection)
        {
            return
                connection.User?.FindFirst("UserId")?.Value
                ?? connection.User?.FindFirst("AdminId")?.Value
                ?? connection.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? connection.User?.FindFirst("sub")?.Value;
        }
    }


    //public class AppUserIdProvider : IUserIdProvider
    //{
    //    public string? GetUserId(HubConnectionContext connection)
    //    {
    //        var user = connection.User;
    //        if (user == null)
    //            return null;

    //        // 🔥 SINGLE SOURCE OF TRUTH
    //        var userId =
    //            user.FindFirst("UserId")?.Value
    //            ?? user.FindFirst(ClaimTypes.NameIdentifier)?.Value
    //            ?? user.FindFirst("sub")?.Value;

    //        // Optional debug (remove in prod if noisy)
    //        if (string.IsNullOrEmpty(userId))
    //        {
    //            Console.WriteLine("[SignalR] UserId could not be resolved from JWT claims.");
    //        }

    //        return userId;
    //    }
    //}
}
