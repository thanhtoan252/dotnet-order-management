using Microsoft.Extensions.Logging;
using OrderManagement.Application.Common.Interfaces;
using OrderManagement.Application.Orders.Mappers;
using OrderManagement.Domain.Common;
using OrderManagement.Domain.Repositories;

namespace OrderManagement.Application.Orders.Commands;

public record DeliverOrderCommand(Guid OrderId, string DeliveredBy)
    : ICommand<Result<OrderResponse>>;

public class DeliverOrderHandler(
    IOrderRepository orderRepo,
    IUnitOfWork uow,
    ILogger<DeliverOrderHandler> logger)
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

        orderRepo.Update(order);
        await uow.SaveChangesAsync(ct);

        logger.LogInformation("Order {OrderNumber} marked as delivered by {User}.", order.OrderNumber, command.DeliveredBy);

        return order.ToCommandResponse();
    }
}
