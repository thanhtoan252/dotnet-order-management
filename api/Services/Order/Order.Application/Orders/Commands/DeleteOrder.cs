using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Order.Application.Abstractions;
using Order.Domain;
using Shared.Contracts;
using Shared.Contracts.IntegrationEvents;
using Shared.Core.CQRS;
using Shared.Core.Domain;
using Shared.Messaging.Abstractions;
using DomainOrderStatus = Order.Domain.Entities.OrderStatus;

namespace Order.Application.Orders.Commands;

public record DeleteOrderCommand(Guid OrderId)
    : ICommand<Result>;

/// <summary>
///     Soft-deletes the order. If Pending with items, publishes OrderCancelled
///     so Catalog Service can restore stock.
/// </summary>
public class DeleteOrderHandler(
    IOrderDbContext db,
    IEventBus eventBus,
    ILogger<DeleteOrderHandler> logger)
    : ICommandHandler<DeleteOrderCommand, Result>
{
    public async Task<Result> HandleAsync(DeleteOrderCommand command, CancellationToken ct)
    {
        var order = await db.Orders.Include(o => o.Items).SingleOrDefaultAsync(o => o.Id == command.OrderId, ct);
        if (order is null)
        {
            return DomainErrors.Order.NotFound(command.OrderId);
        }

        if (order.Status == DomainOrderStatus.Pending && order.Items.Count > 0)
        {
            // Publish event to restore stock in Catalog Service
            var integrationEvent = new OrderCancelledIntegrationEvent(
                Guid.NewGuid(),
                DateTime.UtcNow,
                order.Id,
                order.OrderNumber,
                "Order deleted",
                order.Items.Select(i => new OrderLineItem(i.ProductId, i.Quantity)).ToList());

            await eventBus.PublishAsync(integrationEvent, Topics.OrderCancelled, order.Id.ToString(), ct);

            logger.LogInformation("Order {OrderId} deleted, stock restoration event published for {ItemCount} item(s).",
                command.OrderId, order.Items.Count);
        }
        else
        {
            logger.LogInformation("Order {OrderId} deleted.", command.OrderId);
        }

        order.IsDeleted = true;
        await db.SaveChangesAsync(ct);

        return Result.Success();
    }
}
