using Microsoft.Extensions.Logging;
using OrderManagement.Application.Common.Interfaces;
using OrderManagement.Application.Orders.Mappers;
using OrderManagement.Domain.Common;
using OrderManagement.Domain.Entities;
using OrderManagement.Domain.Repositories;
using OrderManagement.Domain.ValueObjects;

namespace OrderManagement.Application.Orders.Commands;

public record PlaceOrderCommand(PlaceOrderRequest Request, string PlacedBy)
    : ICommand<Result<OrderResponse>>;

public class PlaceOrderHandler(IOrderRepository orderRepo, IProductRepository productRepo, IUnitOfWork uow, ILogger<PlaceOrderHandler> logger)
    : ICommandHandler<PlaceOrderCommand, Result<OrderResponse>>
{
    public async Task<Result<OrderResponse>> HandleAsync(PlaceOrderCommand command, CancellationToken ct)
    {
        var request = command.Request;

        return await uow.ExecuteInTransactionAsync<OrderResponse>(async () =>
        {
            var productIds = request.Lines.Select(l => l.ProductId).ToList();
            var products = await productRepo.GetByIdsAsync(productIds, ct);

            var missing = productIds.Except(products.Select(p => p.Id)).ToList();
            if (missing.Count != 0)
            {
                return DomainErrors.Order.ProductsNotFound(missing);
            }

            var address = Address.Create(
                request.ShippingAddress.Street,
                request.ShippingAddress.City,
                request.ShippingAddress.Province,
                request.ShippingAddress.ZipCode);

            var createResult = Order.Create(request.CustomerId, address, request.Notes);
            if (createResult.IsFailure)
            {
                return createResult.Error;
            }

            var order = createResult.Value;
            order.CreatedBy = command.PlacedBy;

            foreach (var line in request.Lines)
            {
                var product = products.First(p => p.Id == line.ProductId);
                var addResult = order.AddItem(product, line.Quantity);
                if (addResult.IsFailure)
                {
                    return addResult.Error;
                }

                productRepo.Update(product);
            }

            orderRepo.Add(order);
            await uow.SaveChangesAsync(ct);

            logger.LogInformation(
                "Order {OrderNumber} placed by {User} for customer {CustomerId} with {ItemCount} item(s). Total: {Total}",
                order.OrderNumber, command.PlacedBy, request.CustomerId, order.Items.Count, order.TotalAmount);

            return order.ToCommandResponse();
        }, ct);
    }
}
