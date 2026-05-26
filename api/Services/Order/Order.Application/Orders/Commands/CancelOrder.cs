using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Order.Application.Abstractions;
using Order.Application.Orders.Mappers;
using Order.Application.Orders.Models;
using Order.Domain;
using Shared.Contracts;
using Shared.Contracts.IntegrationEvents;
using Shared.Core.CQRS;
using Shared.Core.Domain;
using Shared.Messaging.Abstractions;

namespace Order.Application.Orders.Commands;

public record CancelOrderCommand(Guid OrderId, string Reason, string CancelledBy, Guid UserId)
    : ICommand<Result<OrderResult>>;

/// <summary>
///     Cancels the order and publishes an OrderCancelled integration event
///     so Catalog Service can restore stock.
/// </summary>
public class CancelOrderHandler(
    IOrderDbContext db,
    IEventBus eventBus,
    ILogger<CancelOrderHandler> logger)
    : ICommandHandler<CancelOrderCommand, Result<OrderResult>>
{
    public async Task<Result<OrderResult>> HandleAsync(CancelOrderCommand command, CancellationToken ct)
    {
        var order = await db.Orders.Include(o => o.Items).SingleOrDefaultAsync(o => o.Id == command.OrderId, ct);
        if (order is null)
        {
            return DomainErrors.Order.NotFound(command.OrderId);
        }

        var cancelResult = order.Cancel(command.Reason, command.CancelledBy);
        if (cancelResult.IsFailure)
        {
            return cancelResult.Error;
        }

        // Publish integration event for Catalog Service to restore stock
        var integrationEvent = new OrderCancelledIntegrationEvent(
            Guid.NewGuid(),
            DateTime.UtcNow,
            order.Id,
            order.OrderNumber,
            command.UserId,
            command.Reason,
            order.Items.Select(i => new OrderLineItem(i.ProductId, i.Quantity)).ToList());

        await eventBus.PublishAsync(integrationEvent, Topics.OrderCancelled, order.Id.ToString(), ct);

        await db.SaveChangesAsync(ct);

        logger.LogInformation("Order {OrderNumber} cancelled by {User}. Reason: {Reason}",
            order.OrderNumber, command.CancelledBy, command.Reason);

        return order.ToResult();
    }
}
