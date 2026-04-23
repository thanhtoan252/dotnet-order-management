using Inventory.Application.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared.Contracts.IntegrationEvents;
using Shared.Core.CQRS;
using Shared.Messaging.Abstractions;

namespace Inventory.Application.EventHandlers;

public class ProductDeletedConsumer(
    IInventoryDbContext db,
    ILogger<ProductDeletedConsumer> logger)
    : IEventConsumer<ProductDeletedIntegrationEvent>
{
    public async Task HandleAsync(ProductDeletedIntegrationEvent @event, CancellationToken ct = default)
    {
        var item = await db.InventoryItems.SingleOrDefaultAsync(i => i.ProductId == @event.ProductId, ct);
        if (item is null)
        {
            logger.LogInformation("No inventory item found for deleted product {ProductId}, skipping.",
                @event.ProductId);
            return;
        }

        item.IsDeleted = true;
        db.InventoryItems.Update(item);
        await db.SaveChangesAsync(ct);

        logger.LogInformation("Inventory item soft-deleted for product {ProductId}.", @event.ProductId);
    }
}
