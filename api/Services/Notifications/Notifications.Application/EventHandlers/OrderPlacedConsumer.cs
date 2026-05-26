using System.Text.Json;
using Microsoft.Extensions.Logging;
using Notifications.Application.Abstractions;
using Notifications.Application.Notifications.Mappers;
using Notifications.Application.Realtime;
using Notifications.Domain.Entities;
using Notifications.Domain.Enums;
using Shared.Contracts;
using Shared.Contracts.IntegrationEvents;
using Shared.Messaging.Abstractions;

namespace Notifications.Application.EventHandlers;

public sealed class OrderPlacedConsumer(
    INotificationDbContext db,
    INotificationPusher pusher,
    ILogger<OrderPlacedConsumer> logger)
    : IEventConsumer<OrderPlacedIntegrationEvent>
{
    public static string Topic => Topics.OrderPlaced;

    public async Task HandleAsync(OrderPlacedIntegrationEvent @event, CancellationToken ct = default)
    {
        if (@event.UserId == Guid.Empty)
        {
            logger.LogWarning("OrderPlaced event {OrderId} has no UserId; skipping notification.", @event.OrderId);
            return;
        }

        var values = new Dictionary<string, string?>
        {
            ["OrderId"] = @event.OrderId.ToString(),
            ["OrderNumber"] = @event.OrderNumber,
            ["CustomerId"] = @event.CustomerId.ToString()
        };

        var rendered = await NotificationFactory.RenderAsync(db, NotificationType.OrderPlaced, values, ct);
        if (rendered is null)
        {
            logger.LogWarning("No active template for OrderPlaced; skipping.");
            return;
        }

        var metadata = JsonSerializer.Serialize(new { @event.OrderId, @event.OrderNumber });
        var notification = Notification.ForUser(@event.UserId, NotificationType.OrderPlaced,
            rendered.Value.Title, rendered.Value.Body, @event.OrderId, metadata);

        db.Notifications.Add(notification);
        await db.SaveChangesAsync(ct);

        await pusher.PushToUserAsync(@event.UserId, notification.ToResponse(), ct);
    }
}
