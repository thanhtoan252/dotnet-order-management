using System.Security.Claims;
using Microsoft.AspNetCore.SignalR;

namespace Notifications.Infrastructure.Realtime;

public sealed class SubClaimUserIdProvider : IUserIdProvider
{
    public string? GetUserId(HubConnectionContext connection)
    {
        return connection.User?.FindFirstValue("sub");
    }
}
