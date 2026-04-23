using Inventory.Domain.Repositories;
using Microsoft.Extensions.Logging;
using Shared.Contracts.IntegrationEvents;
using Shared.Core.CQRS;
using Shared.Messaging.Abstractions;

namespace Inventory.Application.EventHandlers;

public class ProductRenamedConsumer(
    IInventoryRepository inventoryRepo,
    IUnitOfWork uow,
    ILogger<ProductRenamedConsumer> logger)
    : IEventConsumer<ProductRenamedIntegrationEvent>
{
    public async Task HandleAsync(ProductRenamedIntegrationEvent @event, CancellationToken ct = default)
    {
        var item = await inventoryRepo.GetByProductIdAsync(@event.ProductId, ct);
        if (item is null)
        {
            logger.LogInformation("No inventory item found for renamed product {ProductId}, skipping.",
                @event.ProductId);
            return;
        }

        item.RenameProduct(@event.NewName);
        inventoryRepo.Update(item);
        await uow.SaveChangesAsync(ct);

        logger.LogInformation("Inventory item renamed for product {ProductId} -> {NewName}.",
            @event.ProductId, @event.NewName);
    }
}
