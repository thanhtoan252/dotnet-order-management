using Microsoft.Extensions.Logging;
using Order.Application.Orders.Mappers;
using Order.Domain.Repositories;
using Shared.Contracts.IntegrationEvents;
using Shared.Core.CQRS;
using Shared.Core.ValueObjects;
using Shared.Messaging.Abstractions;

namespace Order.Application.EventHandlers;

/// <summary>
///     Handles StockReserved from Catalog Service — auto-confirms the order
///     and updates item snapshots with confirmed product details.
/// </summary>
public class StockReservedConsumer(
    IOrderRepository orderRepo,
    IUnitOfWork uow,
    ILogger<StockReservedConsumer> logger)
    : IEventConsumer<StockReservedIntegrationEvent>
{
    public async Task HandleAsync(StockReservedIntegrationEvent @event, CancellationToken ct = default)
    {
        logger.LogInformation("Stock reserved for order {OrderId}, auto-confirming", @event.OrderId);

        var order = await orderRepo.GetByIdWithItemsAsync(@event.OrderId, ct);
        if (order is null)
        {
            logger.LogWarning("Order {OrderId} not found when processing StockReserved event", @event.OrderId);
            return;
        }

        var confirmResult = order.Confirm("system:stock-reserved");
        if (confirmResult.IsFailure)
        {
            logger.LogWarning("Cannot confirm order {OrderId}: {Error}", @event.OrderId, confirmResult.Error.Message);
            return;
        }

        await uow.SaveChangesAsync(ct);

        logger.LogInformation("Order {OrderNumber} auto-confirmed after stock reservation", order.OrderNumber);
    }
}
