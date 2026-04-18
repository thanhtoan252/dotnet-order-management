using Inventory.Domain.Repositories;
using Microsoft.Extensions.Logging;
using Shared.Contracts.IntegrationEvents;
using Shared.Core.CQRS;
using Shared.Messaging.Abstractions;

namespace Inventory.Application.EventHandlers;

public class ProductDeletedConsumer(
    IInventoryRepository inventoryRepo,
    IUnitOfWork uow,
    ILogger<ProductDeletedConsumer> logger)
    : IEventConsumer<ProductDeletedIntegrationEvent>
{
    public async Task HandleAsync(ProductDeletedIntegrationEvent @event, CancellationToken ct = default)
    {
        var item = await inventoryRepo.GetByProductIdAsync(@event.ProductId, ct);
        if (item is null)
        {
            logger.LogInformation("No inventory item found for deleted product {ProductId}, skipping.",
                @event.ProductId);
            return;
        }

        item.IsDeleted = true;
        inventoryRepo.Update(item);
        await uow.SaveChangesAsync(ct);

        logger.LogInformation("Inventory item soft-deleted for product {ProductId}.", @event.ProductId);
    }
}
