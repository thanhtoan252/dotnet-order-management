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

public sealed class ProductRenamedConsumer(INotificationDbContext db, INotificationPusher pusher)
    : IEventConsumer<ProductRenamedIntegrationEvent>
{
    public static string Topic => Topics.ProductRenamed;

    public async Task HandleAsync(ProductRenamedIntegrationEvent @event, CancellationToken ct = default)
    {
        var values = new Dictionary<string, string?>
        {
            ["ProductId"] = @event.ProductId.ToString(),
            ["NewName"] = @event.NewName
        };

        var rendered = await NotificationFactory.RenderAsync(db, NotificationType.ProductRenamed, values, ct);
        if (rendered is null)
        {
            return;
        }

        var metadata = JsonSerializer.Serialize(new { @event.ProductId, @event.NewName });
        var notification = Notification.ForAdmins(NotificationType.ProductRenamed,
            rendered.Value.Title, rendered.Value.Body, metadata);

        db.Notifications.Add(notification);
        await db.SaveChangesAsync(ct);

        await pusher.PushToAdminsAsync(notification.ToResponse(), ct);
    }
}
