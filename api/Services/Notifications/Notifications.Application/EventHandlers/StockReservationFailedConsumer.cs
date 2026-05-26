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

public sealed class StockReservationFailedConsumer(
    INotificationDbContext db,
    INotificationPusher pusher,
    ILogger<StockReservationFailedConsumer> logger)
    : IEventConsumer<StockReservationFailedIntegrationEvent>
{
    public static string Topic => Topics.StockReservationFailed;

    public async Task HandleAsync(StockReservationFailedIntegrationEvent @event, CancellationToken ct = default)
    {
        var userId = await NotificationFactory.ResolveUserIdForOrderAsync(db, @event.OrderId, ct);
        if (userId is null)
        {
            logger.LogWarning("No OrderPlaced notification found for order {OrderId}; cannot send rejection.",
                @event.OrderId);
            return;
        }

        var values = new Dictionary<string, string?>
        {
            ["OrderId"] = @event.OrderId.ToString(),
            ["Reason"] = @event.Reason
        };

        var rendered = await NotificationFactory.RenderAsync(db, NotificationType.StockReservationFailed, values, ct);
        if (rendered is null)
        {
            return;
        }

        var metadata = JsonSerializer.Serialize(new { @event.OrderId, @event.Reason });
        var notification = Notification.ForUser(userId.Value, NotificationType.StockReservationFailed,
            rendered.Value.Title, rendered.Value.Body, @event.OrderId, metadata);

        db.Notifications.Add(notification);
        await db.SaveChangesAsync(ct);

        await pusher.PushToUserAsync(userId.Value, notification.ToResponse(), ct);
    }
}
