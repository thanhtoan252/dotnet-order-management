using System.Text.Json;
using Notifications.Application.Abstractions;
using Notifications.Application.Notifications.Mappers;
using Notifications.Application.Realtime;
using Notifications.Domain.Entities;
using Notifications.Domain.Enums;
using Shared.Contracts;
using Shared.Contracts.IntegrationEvents;
using Shared.Messaging.Abstractions;

namespace Notifications.Application.EventHandlers;

public sealed class ProductDeletedConsumer(INotificationDbContext db, INotificationPusher pusher)
    : IEventConsumer<ProductDeletedIntegrationEvent>
{
    public static string Topic => Topics.ProductDeleted;

    public async Task HandleAsync(ProductDeletedIntegrationEvent @event, CancellationToken ct = default)
    {
        var values = new Dictionary<string, string?>
        {
            ["ProductId"] = @event.ProductId.ToString()
        };

        var rendered = await NotificationFactory.RenderAsync(db, NotificationType.ProductDeleted, values, ct);
        if (rendered is null)
        {
            return;
        }

        var metadata = JsonSerializer.Serialize(new { @event.ProductId });
        var notification = Notification.ForAdmins(NotificationType.ProductDeleted,
            rendered.Value.Title, rendered.Value.Body, metadata);

        db.Notifications.Add(notification);
        await db.SaveChangesAsync(ct);

        await pusher.PushToAdminsAsync(notification.ToResponse(), ct);
    }
}
