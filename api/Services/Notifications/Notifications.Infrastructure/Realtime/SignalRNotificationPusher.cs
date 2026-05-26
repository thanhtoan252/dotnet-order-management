using Microsoft.AspNetCore.SignalR;
using Notifications.Application.Notifications.Mappers;
using Notifications.Application.Realtime;

namespace Notifications.Infrastructure.Realtime;

public sealed class SignalRNotificationPusher(IHubContext<NotificationHub> hub) : INotificationPusher
{
    public Task PushToUserAsync(Guid userId, NotificationResponse notification, CancellationToken ct = default)
    {
        return hub.Clients.User(userId.ToString())
            .SendAsync(HubMethods.ReceiveNotification, notification, ct);
    }

    public Task PushToAdminsAsync(NotificationResponse notification, CancellationToken ct = default)
    {
        return hub.Clients.Group(NotificationHub.AdminsGroup)
            .SendAsync(HubMethods.ReceiveNotification, notification, ct);
    }

    public Task PushToAllAsync(NotificationResponse notification, CancellationToken ct = default)
    {
        return hub.Clients.All
            .SendAsync(HubMethods.ReceiveNotification, notification, ct);
    }
}
