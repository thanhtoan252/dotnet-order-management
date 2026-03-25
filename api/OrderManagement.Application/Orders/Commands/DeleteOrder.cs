using Microsoft.Extensions.Logging;
using OrderManagement.Application.Common.Interfaces;
using OrderManagement.Domain.Common;
using OrderManagement.Domain.Repositories;
using DomainOrderStatus = OrderManagement.Domain.Entities.OrderStatus;

namespace OrderManagement.Application.Orders.Commands;

public record DeleteOrderCommand(Guid OrderId)
    : ICommand<Result>;

public class DeleteOrderHandler(
    IOrderRepository orderRepo,
    IProductRepository productRepo,
    IUnitOfWork uow,
    ILogger<DeleteOrderHandler> logger)
    : ICommandHandler<DeleteOrderCommand, Result>
{
    public async Task<Result> HandleAsync(DeleteOrderCommand command, CancellationToken ct)
    {
        var order = await orderRepo.GetByIdWithItemsAsync(command.OrderId, ct);
        if (order is null)
        {
            return DomainErrors.Order.NotFound(command.OrderId);
        }

        if (order.Status == DomainOrderStatus.Pending && order.Items.Count > 0)
        {
            var productIds = order.Items.Select(i => i.ProductId).ToList();
            var products = await productRepo.GetByIdsAsync(productIds, ct);

            await uow.ExecuteInTransactionAsync(async () =>
            {
                foreach (var item in order.Items)
                {
                    var product = products.FirstOrDefault(p => p.Id == item.ProductId);
                    if (product is not null)
                    {
                        product.RestoreStock(item.Quantity);
                        productRepo.Update(product);
                    }
                }

                await SoftDeleteAsync();
            }, ct);

            logger.LogInformation("Order {OrderId} deleted, stock restored for {ItemCount} item(s).",
                command.OrderId, order.Items.Count);
        }
        else
        {
            await SoftDeleteAsync();
            logger.LogInformation("Order {OrderId} deleted.", command.OrderId);
        }

        return Result.Success();

        async Task SoftDeleteAsync()
        {
            order.IsDeleted = true;
            orderRepo.Update(order);
            await uow.SaveChangesAsync(ct);
        }
    }
}
