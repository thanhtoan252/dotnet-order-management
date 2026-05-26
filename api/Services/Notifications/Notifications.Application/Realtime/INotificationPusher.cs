using Notifications.Application.Notifications.Mappers;

namespace Notifications.Application.Realtime;

public interface INotificationPusher
{
    Task PushToUserAsync(Guid userId, NotificationResponse notification, CancellationToken ct = default);
    Task PushToAdminsAsync(NotificationResponse notification, CancellationToken ct = default);
    Task PushToAllAsync(NotificationResponse notification, CancellationToken ct = default);
}
