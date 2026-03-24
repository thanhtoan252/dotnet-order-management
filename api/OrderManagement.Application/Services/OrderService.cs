using Microsoft.Extensions.Logging;
using OrderManagement.Application.Common.Interfaces;
using OrderManagement.Domain.Common;
using OrderManagement.Domain.Entities;
using OrderManagement.Domain.Repositories;
using OrderManagement.Domain.ValueObjects;

namespace OrderManagement.Application.Services;

public class OrderService(
    IOrderRepository orderRepo,
    IProductRepository productRepo,
    IUnitOfWork uow,
    ILogger<OrderService> logger)
{
    // ─── Write operations ─────────────────────────────────────────────────────

    public async Task<Result<Order>> PlaceOrderAsync(Guid customerId, Address shippingAddress,
        IReadOnlyList<(Guid ProductId, int Quantity)> lines, string? notes, string placedBy,
        CancellationToken ct = default)
    {
        return await uow.ExecuteInTransactionAsync<Order>(async () =>
        {
            var productIds = lines.Select(l => l.ProductId).ToList();
            var products = await productRepo.GetByIdsAsync(productIds, ct);

            var missing = productIds.Except(products.Select(p => p.Id)).ToList();
            if (missing.Count != 0)
            {
                return DomainErrors.Order.ProductsNotFound(missing);
            }

            var createResult = Order.Create(customerId, shippingAddress, notes);
            if (createResult.IsFailure)
            {
                return createResult.Error;
            }

            var order = createResult.Value;
            order.CreatedBy = placedBy;

            foreach (var (productId, quantity) in lines)
            {
                var product = products.First(p => p.Id == productId);
                var addResult = order.AddItem(product, quantity);
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
                order.OrderNumber, placedBy, customerId, order.Items.Count, order.TotalAmount);

            return order;
        }, ct);
    }

    public async Task<Result<Order>> ConfirmOrderAsync(Guid orderId, string confirmedBy, CancellationToken ct = default)
    {
        var order = await orderRepo.GetByIdWithItemsAsync(orderId, ct);
        if (order is null)
        {
            return DomainErrors.Order.NotFound(orderId);
        }

        var result = order.Confirm(confirmedBy);
        if (result.IsFailure)
        {
            return result.Error;
        }

        orderRepo.Update(order);
        await uow.SaveChangesAsync(ct);

        logger.LogInformation("Order {OrderNumber} confirmed by {User}.", order.OrderNumber, confirmedBy);
        return order;
    }

    public async Task<Result<Order>> ShipOrderAsync(Guid orderId, string shippedBy, CancellationToken ct = default)
    {
        var order = await orderRepo.GetByIdWithItemsAsync(orderId, ct);
        if (order is null)
        {
            return DomainErrors.Order.NotFound(orderId);
        }

        var result = order.Ship(shippedBy);
        if (result.IsFailure)
        {
            return result.Error;
        }

        orderRepo.Update(order);
        await uow.SaveChangesAsync(ct);

        logger.LogInformation("Order {OrderNumber} shipped by {User}.", order.OrderNumber, shippedBy);
        return order;
    }

    public async Task<Result<Order>> CancelOrderAsync(Guid orderId, string reason, string cancelledBy,
        CancellationToken ct = default)
    {
        var order = await orderRepo.GetByIdWithItemsAsync(orderId, ct);
        if (order is null)
        {
            return DomainErrors.Order.NotFound(orderId);
        }

        return await uow.ExecuteInTransactionAsync<Order>(async () =>
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

            var cancelResult = order.Cancel(reason, cancelledBy);
            if (cancelResult.IsFailure)
            {
                return cancelResult.Error;
            }

            orderRepo.Update(order);
            await uow.SaveChangesAsync(ct);

            logger.LogInformation("Order {OrderNumber} cancelled by {User}. Reason: {Reason}",
                order.OrderNumber, cancelledBy, reason);

            return order;
        }, ct);
    }

    public async Task<Result<Order>> DeliverOrderAsync(Guid orderId, string deliveredBy, CancellationToken ct = default)
    {
        var order = await orderRepo.GetByIdWithItemsAsync(orderId, ct);
        if (order is null)
        {
            return DomainErrors.Order.NotFound(orderId);
        }

        var result = order.MarkDelivered();
        if (result.IsFailure)
        {
            return result.Error;
        }

        orderRepo.Update(order);
        await uow.SaveChangesAsync(ct);

        logger.LogInformation("Order {OrderNumber} marked as delivered by {User}.", order.OrderNumber, deliveredBy);
        return order;
    }

    public async Task<Result<Order>> GetOrderAsync(Guid orderId, CancellationToken ct = default)
    {
        var order = await orderRepo.GetByIdWithItemsAsync(orderId, ct);
        if (order is null)
        {
            return DomainErrors.Order.NotFound(orderId);
        }
        return order;
    }

    public async Task<Result> DeleteOrderAsync(Guid orderId, CancellationToken ct = default)
    {
        var order = await orderRepo.GetByIdWithItemsAsync(orderId, ct);
        if (order is null)
        {
            return DomainErrors.Order.NotFound(orderId);
        }

        if (order.Status == OrderStatus.Pending && order.Items.Count > 0)
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

                order.IsDeleted = true;
                orderRepo.Update(order);
                await uow.SaveChangesAsync(ct);
            }, ct);

            logger.LogInformation("Order {OrderId} deleted, stock restored for {ItemCount} item(s).", orderId,
                order.Items.Count);
            return Result.Success();
        }

        order.IsDeleted = true;
        orderRepo.Update(order);
        await uow.SaveChangesAsync(ct);

        logger.LogInformation("Order {OrderId} deleted.", orderId);
        return Result.Success();
    }

    // ─── Read operations (cannot fail) ────────────────────────────────────────

    public async Task<IReadOnlyList<Order>> GetCustomerOrdersAsync(Guid customerId, int page = 1, int pageSize = 20,
        CancellationToken ct = default)
    {
        return await orderRepo.GetByCustomerIdAsync(customerId, page, pageSize, ct);
    }

    public async Task<IReadOnlyList<Order>> GetAllOrdersAsync(int page = 1, int pageSize = 100,
        CancellationToken ct = default)
    {
        return await orderRepo.GetAllAsync(page, pageSize, ct);
    }
}
