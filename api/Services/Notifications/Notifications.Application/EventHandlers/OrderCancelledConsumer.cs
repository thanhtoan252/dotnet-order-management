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

public sealed class OrderCancelledConsumer(
    INotificationDbContext db,
    INotificationPusher pusher,
    ILogger<OrderCancelledConsumer> logger)
    : IEventConsumer<OrderCancelledIntegrationEvent>
{
    public static string Topic => Topics.OrderCancelled;

    public async Task HandleAsync(OrderCancelledIntegrationEvent @event, CancellationToken ct = default)
    {
        var userId = @event.UserId == Guid.Empty
            ? await NotificationFactory.ResolveUserIdForOrderAsync(db, @event.OrderId, ct)
            : @event.UserId;

        if (userId is null || userId == Guid.Empty)
        {
            logger.LogWarning(
                "Cannot resolve user for cancelled order {OrderId}; skipping notification.",
                @event.OrderId);
            return;
        }

        var values = new Dictionary<string, string?>
        {
            ["OrderId"] = @event.OrderId.ToString(),
            ["OrderNumber"] = @event.OrderNumber,
            ["Reason"] = @event.Reason
        };

        var rendered = await NotificationFactory.RenderAsync(db, NotificationType.OrderCancelled, values, ct);
        if (rendered is null)
        {
            return;
        }

        var metadata = JsonSerializer.Serialize(new { @event.OrderId, @event.OrderNumber, @event.Reason });
        var notification = Notification.ForUser(userId.Value, NotificationType.OrderCancelled,
            rendered.Value.Title, rendered.Value.Body, @event.OrderId, metadata);

        db.Notifications.Add(notification);
        await db.SaveChangesAsync(ct);

        await pusher.PushToUserAsync(userId.Value, notification.ToResponse(), ct);
    }
}
