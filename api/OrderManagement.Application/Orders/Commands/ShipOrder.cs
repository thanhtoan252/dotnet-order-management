using Microsoft.Extensions.Logging;
using OrderManagement.Application.Common.Interfaces;
using OrderManagement.Application.Orders.Mappers;
using OrderManagement.Domain.Common;
using OrderManagement.Domain.Repositories;

namespace OrderManagement.Application.Orders.Commands;

public record ShipOrderCommand(Guid OrderId, string ShippedBy)
    : ICommand<Result<OrderResponse>>;

public class ShipOrderHandler(IOrderRepository orderRepo, IUnitOfWork uow, ILogger<ShipOrderHandler> logger)
    : ICommandHandler<ShipOrderCommand, Result<OrderResponse>>
{
    public async Task<Result<OrderResponse>> HandleAsync(ShipOrderCommand command, CancellationToken ct)
    {
        var order = await orderRepo.GetByIdWithItemsAsync(command.OrderId, ct);
        if (order is null)
        {
            return DomainErrors.Order.NotFound(command.OrderId);
        }

        var result = order.Ship(command.ShippedBy);
        if (result.IsFailure)
        {
            return result.Error;
        }

        orderRepo.Update(order);
        await uow.SaveChangesAsync(ct);

        logger.LogInformation("Order {OrderNumber} shipped by {User}.", order.OrderNumber, command.ShippedBy);

        return order.ToCommandResponse();
    }
}
