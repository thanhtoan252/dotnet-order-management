using Microsoft.Extensions.Logging;
using Order.Application.Orders.Mappers;
using Order.Domain;
using Order.Domain.Repositories;
using Shared.Core.CQRS;
using Shared.Core.Domain;

namespace Order.Application.Orders.Commands;

public record DeliverOrderCommand(Guid OrderId, string DeliveredBy)
    : ICommand<Result<OrderResponse>>;

public class DeliverOrderHandler(IOrderRepository orderRepo, IUnitOfWork uow, ILogger<DeliverOrderHandler> logger)
    : ICommandHandler<DeliverOrderCommand, Result<OrderResponse>>
{
    public async Task<Result<OrderResponse>> HandleAsync(DeliverOrderCommand command, CancellationToken ct)
    {
        var order = await orderRepo.GetByIdWithItemsAsync(command.OrderId, ct);
        if (order is null)
        {
            return DomainErrors.Order.NotFound(command.OrderId);
        }

        var result = order.MarkDelivered();
        if (result.IsFailure)
        {
            return result.Error;
        }

        await uow.SaveChangesAsync(ct);

        logger.LogInformation("Order {OrderNumber} marked as delivered by {User}.", order.OrderNumber, command.DeliveredBy);

        return order.ToCommandResponse();
    }
}
