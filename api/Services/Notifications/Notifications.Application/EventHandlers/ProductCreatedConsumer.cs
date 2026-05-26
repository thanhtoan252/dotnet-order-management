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

public sealed class ProductCreatedConsumer(INotificationDbContext db, INotificationPusher pusher)
    : IEventConsumer<ProductCreatedIntegrationEvent>
{
    public static string Topic => Topics.ProductCreated;

    public async Task HandleAsync(ProductCreatedIntegrationEvent @event, CancellationToken ct = default)
    {
        var values = new Dictionary<string, string?>
        {
            ["ProductId"] = @event.ProductId.ToString(),
            ["Sku"] = @event.Sku,
            ["Name"] = @event.Name,
            ["InitialStockQuantity"] = @event.InitialStockQuantity.ToString()
        };

        var rendered = await NotificationFactory.RenderAsync(db, NotificationType.ProductCreated, values, ct);
        if (rendered is null)
        {
            return;
        }

        var metadata = JsonSerializer.Serialize(new { @event.ProductId, @event.Sku, @event.Name });
        var notification = Notification.ForAdmins(NotificationType.ProductCreated,
            rendered.Value.Title, rendered.Value.Body, metadata);

        db.Notifications.Add(notification);
        await db.SaveChangesAsync(ct);

        await pusher.PushToAdminsAsync(notification.ToResponse(), ct);
    }
}
