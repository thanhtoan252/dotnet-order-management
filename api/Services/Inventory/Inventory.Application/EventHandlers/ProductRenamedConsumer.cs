using Inventory.Application.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared.Contracts.IntegrationEvents;
using Shared.Core.CQRS;
using Shared.Messaging.Abstractions;

namespace Inventory.Application.EventHandlers;

public class ProductRenamedConsumer(
    IInventoryDbContext db,
    ILogger<ProductRenamedConsumer> logger)
    : IEventConsumer<ProductRenamedIntegrationEvent>
{
    public async Task HandleAsync(ProductRenamedIntegrationEvent @event, CancellationToken ct = default)
    {
        var item = await db.InventoryItems.SingleOrDefaultAsync(i => i.ProductId == @event.ProductId, ct);
        if (item is null)
        {
            logger.LogInformation("No inventory item found for renamed product {ProductId}, skipping.",
                @event.ProductId);
            return;
        }

        item.RenameProduct(@event.NewName);
        db.InventoryItems.Update(item);
        await db.SaveChangesAsync(ct);

        logger.LogInformation("Inventory item renamed for product {ProductId} -> {NewName}.",
            @event.ProductId, @event.NewName);
    }
}
