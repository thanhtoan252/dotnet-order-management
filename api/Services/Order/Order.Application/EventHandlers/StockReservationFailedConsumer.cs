using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Order.Application.Abstractions;
using Shared.Contracts.IntegrationEvents;
using Shared.Core.CQRS;
using Shared.Messaging.Abstractions;

namespace Order.Application.EventHandlers;

/// <summary>
///     Handles StockReservationFailed from Catalog Service — auto-cancels the order.
/// </summary>
public class StockReservationFailedConsumer(
    IOrderDbContext db,
    ILogger<StockReservationFailedConsumer> logger)
    : IEventConsumer<StockReservationFailedIntegrationEvent>
{
    public async Task HandleAsync(StockReservationFailedIntegrationEvent @event, CancellationToken ct = default)
    {
        logger.LogWarning("Stock reservation failed for order {OrderId}: {Reason}", @event.OrderId, @event.Reason);

        var order = await db.Orders.Include(o => o.Items).SingleOrDefaultAsync(o => o.Id == @event.OrderId, ct);
        if (order is null)
        {
            logger.LogWarning("Order {OrderId} not found when processing StockReservationFailed event", @event.OrderId);
            return;
        }

        var cancelResult = order.Cancel($"Stock reservation failed: {@event.Reason}", "system:stock-failed");
        if (cancelResult.IsFailure)
        {
            logger.LogWarning("Cannot cancel order {OrderId}: {Error}", @event.OrderId, cancelResult.Error.Message);
            return;
        }

        await db.SaveChangesAsync(ct);

        logger.LogInformation("Order {OrderNumber} auto-cancelled due to stock reservation failure", order.OrderNumber);
    }
}
