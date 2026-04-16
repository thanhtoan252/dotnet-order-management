using Catalog.Domain;
using Catalog.Domain.Repositories;
using Microsoft.Extensions.Logging;
using Shared.Contracts;
using Shared.Contracts.IntegrationEvents;
using Shared.Core.CQRS;
using Shared.Messaging.Abstractions;

namespace Catalog.Application.EventHandlers;

public class OrderPlacedConsumer(
    IProductRepository productRepo,
    IUnitOfWork uow,
    IEventBus eventBus,
    ILogger<OrderPlacedConsumer> logger)
    : IEventConsumer<OrderPlacedIntegrationEvent>
{
    public async Task HandleAsync(OrderPlacedIntegrationEvent @event, CancellationToken ct = default)
    {
        logger.LogInformation("Reserving stock for order {OrderId} ({OrderNumber})", @event.OrderId, @event.OrderNumber);

        var productIds = @event.Items.Select(i => i.ProductId).ToList();
        var products = await productRepo.GetByIdsAsync(productIds, ct);
        var productMap = products.ToDictionary(p => p.Id);

        // Validate all products exist and have sufficient stock before deducting
        foreach (var item in @event.Items)
        {
            if (!productMap.TryGetValue(item.ProductId, out var product))
            {
                await PublishFailure(@event.OrderId, $"Product {item.ProductId} not found", ct);
                return;
            }

            if (product.StockQuantity < item.Quantity)
            {
                await PublishFailure(@event.OrderId,
                    DomainErrors.Product.InsufficientStock(product.Name, product.StockQuantity, item.Quantity).Message,
                    ct);
                return;
            }
        }

        // All validations passed — deduct stock
        var reservedItems = new List<ReservedItem>();
        foreach (var item in @event.Items)
        {
            var product = productMap[item.ProductId];
            product.DeductStock(item.Quantity);
            productRepo.Update(product);
            reservedItems.Add(new ReservedItem(
                product.Id,
                product.Name,
                product.Price.Amount,
                product.Price.Currency,
                item.Quantity));
        }

        await eventBus.PublishAsync(
            new StockReservedIntegrationEvent(
                Guid.NewGuid(),
                DateTime.UtcNow,
                @event.OrderId,
                reservedItems),
            Topics.StockReserved,
            @event.OrderId.ToString(),
            ct);

        // Single save: stock deductions + outbox message in one transaction
        await uow.SaveChangesAsync(ct);

        logger.LogInformation("Stock reserved for order {OrderId}", @event.OrderId);
    }

    private async Task PublishFailure(Guid orderId, string reason, CancellationToken ct)
    {
        logger.LogWarning("Stock reservation failed for order {OrderId}: {Reason}", orderId, reason);

        await eventBus.PublishAsync(
            new StockReservationFailedIntegrationEvent(
                Guid.NewGuid(),
                DateTime.UtcNow,
                orderId,
                reason),
            Topics.StockReservationFailed,
            orderId.ToString(),
            ct);

        await uow.SaveChangesAsync(ct);
    }
}
