using Inventory.Domain.Entities;
using Inventory.Domain.Repositories;
using Microsoft.Extensions.Logging;
using Shared.Contracts.IntegrationEvents;
using Shared.Core.CQRS;
using Shared.Messaging.Abstractions;

namespace Inventory.Application.EventHandlers;

public class ProductCreatedConsumer(
    IInventoryRepository inventoryRepo,
    IUnitOfWork uow,
    ILogger<ProductCreatedConsumer> logger)
    : IEventConsumer<ProductCreatedIntegrationEvent>
{
    public async Task HandleAsync(ProductCreatedIntegrationEvent @event, CancellationToken ct = default)
    {
        if (await inventoryRepo.ExistsForProductAsync(@event.ProductId, ct))
        {
            logger.LogInformation("Inventory item already exists for product {ProductId}, skipping create.",
                @event.ProductId);
            return;
        }

        var itemResult = InventoryItem.Create(
            @event.ProductId,
            @event.Sku,
            @event.Name,
            @event.InitialStockQuantity);

        if (itemResult.IsFailure)
        {
            logger.LogWarning("Failed to create inventory item for product {ProductId}: {Error}",
                @event.ProductId, itemResult.Error.Message);
            return;
        }

        inventoryRepo.Add(itemResult.Value);
        await uow.SaveChangesAsync(ct);

        logger.LogInformation("Inventory item created for product {ProductId} (Sku {Sku}, OnHand {OnHand}).",
            @event.ProductId, @event.Sku, @event.InitialStockQuantity);
    }
}
