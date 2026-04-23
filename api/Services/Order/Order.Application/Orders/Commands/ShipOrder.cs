using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Order.Application.Abstractions;
using Order.Application.Orders.Mappers;
using Order.Domain;
using Shared.Core.CQRS;
using Shared.Core.Domain;

namespace Order.Application.Orders.Commands;

public record ShipOrderCommand(Guid OrderId, string ShippedBy)
    : ICommand<Result<OrderResponse>>;

public class ShipOrderHandler(IOrderDbContext db, ILogger<ShipOrderHandler> logger)
    : ICommandHandler<ShipOrderCommand, Result<OrderResponse>>
{
    public async Task<Result<OrderResponse>> HandleAsync(ShipOrderCommand command, CancellationToken ct)
    {
        var order = await db.Orders.Include(o => o.Items).SingleOrDefaultAsync(o => o.Id == command.OrderId, ct);
        if (order is null)
        {
            return DomainErrors.Order.NotFound(command.OrderId);
        }

        var result = order.Ship(command.ShippedBy);
        if (result.IsFailure)
        {
            return result.Error;
        }

        await db.SaveChangesAsync(ct);

        logger.LogInformation("Order {OrderNumber} shipped by {User}.", order.OrderNumber, command.ShippedBy);

        return order.ToCommandResponse();
    }
}
