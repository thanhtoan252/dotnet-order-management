using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Notifications.Infrastructure.Realtime;

[Authorize]
public class NotificationHub : Hub
{
    public const string AdminsGroup = "admins";

    public override async Task OnConnectedAsync()
    {
        if (Context.User?.IsInRole("admin") == true)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, AdminsGroup);
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        if (Context.User?.IsInRole("admin") == true)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, AdminsGroup);
        }

        await base.OnDisconnectedAsync(exception);
    }
}
