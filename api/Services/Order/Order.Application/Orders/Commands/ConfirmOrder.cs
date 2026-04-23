using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Order.Application.Abstractions;
using Order.Application.Orders.Mappers;
using Order.Domain;
using Shared.Core.CQRS;
using Shared.Core.Domain;

namespace Order.Application.Orders.Commands;

public record ConfirmOrderCommand(Guid OrderId, string ConfirmedBy)
    : ICommand<Result<OrderResponse>>;

public class ConfirmOrderHandler(IOrderDbContext db, ILogger<ConfirmOrderHandler> logger)
    : ICommandHandler<ConfirmOrderCommand, Result<OrderResponse>>
{
    public async Task<Result<OrderResponse>> HandleAsync(ConfirmOrderCommand command, CancellationToken ct)
    {
        var order = await db.Orders.Include(o => o.Items).SingleOrDefaultAsync(o => o.Id == command.OrderId, ct);
        if (order is null)
        {
            return DomainErrors.Order.NotFound(command.OrderId);
        }

        var result = order.Confirm(command.ConfirmedBy);
        if (result.IsFailure)
        {
            return result.Error;
        }

        await db.SaveChangesAsync(ct);

        logger.LogInformation("Order {OrderNumber} confirmed by {User}.", order.OrderNumber, command.ConfirmedBy);

        return order.ToCommandResponse();
    }
}
