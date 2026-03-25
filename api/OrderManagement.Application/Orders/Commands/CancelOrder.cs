using Microsoft.Extensions.Logging;
using OrderManagement.Application.Common.Interfaces;
using OrderManagement.Application.Orders.Mappers;
using OrderManagement.Domain.Common;
using OrderManagement.Domain.Repositories;

namespace OrderManagement.Application.Orders.Commands;

public record CancelOrderCommand(Guid OrderId, string Reason, string CancelledBy)
    : ICommand<Result<OrderResponse>>;

public class CancelOrderHandler(IOrderRepository orderRepo, IProductRepository productRepo, IUnitOfWork uow, ILogger<CancelOrderHandler> logger)
    : ICommandHandler<CancelOrderCommand, Result<OrderResponse>>
{
    public async Task<Result<OrderResponse>> HandleAsync(CancelOrderCommand command, CancellationToken ct)
    {
        var order = await orderRepo.GetByIdWithItemsAsync(command.OrderId, ct);
        if (order is null)
        {
            return DomainErrors.Order.NotFound(command.OrderId);
        }

        return await uow.ExecuteInTransactionAsync<OrderResponse>(async () =>
        {
            var productIds = order.Items.Select(i => i.ProductId).ToList();
            var products = await productRepo.GetByIdsAsync(productIds, ct);

            foreach (var item in order.Items)
            {
                var product = products.FirstOrDefault(p => p.Id == item.ProductId);
                if (product is not null)
                {
                    var restoreResult = product.RestoreStock(item.Quantity);
                    if (restoreResult.IsFailure)
                    {
                        return restoreResult.Error;
                    }

                    productRepo.Update(product);
                }
            }

            var cancelResult = order.Cancel(command.Reason, command.CancelledBy);
            if (cancelResult.IsFailure)
            {
                return cancelResult.Error;
            }

            orderRepo.Update(order);
            await uow.SaveChangesAsync(ct);

            logger.LogInformation("Order {OrderNumber} cancelled by {User}. Reason: {Reason}",
                order.OrderNumber, command.CancelledBy, command.Reason);

            return order.ToCommandResponse();
        }, ct);
    }
}
