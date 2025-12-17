using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;
using System.Threading.Tasks;

namespace FactoryOps.NotificationService.Infrastructure.Hubs
{
    [Authorize]
    public class NotificationHub : Hub
    {
        public async override Task OnConnectedAsync()
        {
            var tenantId = Context.User?.FindFirst("tenant-id")?.Value;
            if (tenantId != null)
                await Groups.AddToGroupAsync(Context.ConnectionId, $"tenant-{tenantId}");

            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId != null)
                await Groups.AddToGroupAsync(Context.ConnectionId, $"user-{userId}");

            await base.OnConnectedAsync();
        }

        public Task JoinGroup(string groupName) =>
            Groups.AddToGroupAsync(Context.ConnectionId, groupName);

        public Task LeaveGroup(string groupName) =>
            Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);

        public override async Task OnDisconnectedAsync(System.Exception? exception)
        {
            await base.OnDisconnectedAsync(exception);
        }
    }
}