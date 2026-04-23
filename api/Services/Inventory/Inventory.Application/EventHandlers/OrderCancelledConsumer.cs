using Inventory.Domain.Repositories;
using Microsoft.Extensions.Logging;
using Shared.Contracts.IntegrationEvents;
using Shared.Core.CQRS;
using Shared.Messaging.Abstractions;

namespace Inventory.Application.EventHandlers;

public class OrderCancelledConsumer(
    IInventoryRepository inventoryRepo,
    IUnitOfWork uow,
    ILogger<OrderCancelledConsumer> logger)
    : IEventConsumer<OrderCancelledIntegrationEvent>
{
    public async Task HandleAsync(OrderCancelledIntegrationEvent @event, CancellationToken ct = default)
    {
        logger.LogInformation("Releasing inventory for cancelled order {OrderId} ({OrderNumber})",
            @event.OrderId, @event.OrderNumber);

        var productIds = @event.Items.Select(i => i.ProductId).ToList();
        var items = await inventoryRepo.GetByProductIdsAsync(productIds, ct);
        var itemMap = items.ToDictionary(i => i.ProductId);

        foreach (var line in @event.Items)
        {
            if (!itemMap.TryGetValue(line.ProductId, out var item))
            {
                logger.LogWarning("Inventory item for product {ProductId} not found when releasing for order {OrderId}",
                    line.ProductId, @event.OrderId);
                continue;
            }

            var releaseResult = item.Release(line.Quantity);
            if (releaseResult.IsFailure)
            {
                logger.LogWarning("Could not release inventory for product {ProductId} on order {OrderId}: {Error}",
                    line.ProductId, @event.OrderId, releaseResult.Error.Message);
                continue;
            }

            inventoryRepo.Update(item);
        }

        await uow.SaveChangesAsync(ct);

        logger.LogInformation("Inventory released for cancelled order {OrderId}", @event.OrderId);
    }
}
