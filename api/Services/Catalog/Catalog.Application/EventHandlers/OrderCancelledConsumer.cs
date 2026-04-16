using Catalog.Domain.Repositories;
using Microsoft.Extensions.Logging;
using Shared.Contracts.IntegrationEvents;
using Shared.Core.CQRS;
using Shared.Messaging.Abstractions;

namespace Catalog.Application.EventHandlers;

public class OrderCancelledConsumer(
    IProductRepository productRepo,
    IUnitOfWork uow,
    ILogger<OrderCancelledConsumer> logger)
    : IEventConsumer<OrderCancelledIntegrationEvent>
{
    public async Task HandleAsync(OrderCancelledIntegrationEvent @event, CancellationToken ct = default)
    {
        logger.LogInformation("Restoring stock for cancelled order {OrderId} ({OrderNumber})",
            @event.OrderId, @event.OrderNumber);

        var productIds = @event.Items.Select(i => i.ProductId).ToList();
        var products = await productRepo.GetByIdsAsync(productIds, ct);
        var productMap = products.ToDictionary(p => p.Id);

        foreach (var item in @event.Items)
        {
            if (productMap.TryGetValue(item.ProductId, out var product))
            {
                product.AddStock(item.Quantity);
                productRepo.Update(product);
            }
            else
            {
                logger.LogWarning("Product {ProductId} not found when restoring stock for order {OrderId}",
                    item.ProductId, @event.OrderId);
            }
        }

        await uow.SaveChangesAsync(ct);

        logger.LogInformation("Stock restored for cancelled order {OrderId}", @event.OrderId);
    }
}
