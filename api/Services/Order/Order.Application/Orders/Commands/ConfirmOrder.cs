using Microsoft.Extensions.Logging;
using Order.Application.Orders.Mappers;
using Order.Domain;
using Order.Domain.Repositories;
using Shared.Core.CQRS;
using Shared.Core.Domain;

namespace Order.Application.Orders.Commands;

public record ConfirmOrderCommand(Guid OrderId, string ConfirmedBy)
    : ICommand<Result<OrderResponse>>;

public class ConfirmOrderHandler(IOrderRepository orderRepo, IUnitOfWork uow, ILogger<ConfirmOrderHandler> logger)
    : ICommandHandler<ConfirmOrderCommand, Result<OrderResponse>>
{
    public async Task<Result<OrderResponse>> HandleAsync(ConfirmOrderCommand command, CancellationToken ct)
    {
        var order = await orderRepo.GetByIdWithItemsAsync(command.OrderId, ct);
        if (order is null)
        {
            return DomainErrors.Order.NotFound(command.OrderId);
        }

        var result = order.Confirm(command.ConfirmedBy);
        if (result.IsFailure)
        {
            return result.Error;
        }

        orderRepo.Update(order);
        await uow.SaveChangesAsync(ct);

        logger.LogInformation("Order {OrderNumber} confirmed by {User}.", order.OrderNumber, command.ConfirmedBy);

        return order.ToCommandResponse();
    }
}
