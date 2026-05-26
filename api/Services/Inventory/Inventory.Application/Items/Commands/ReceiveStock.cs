using Inventory.Application.Abstractions;
using Inventory.Application.Items.Mappers;
using Inventory.Application.Items.Models;
using Inventory.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared.Core.CQRS;
using Shared.Core.Domain;

namespace Inventory.Application.Items.Commands;

public record ReceiveStockCommand(Guid ProductId, ReceiveStockInput Request)
    : ICommand<Result<InventoryItemResult>>;

public class ReceiveStockHandler(IInventoryDbContext db, ILogger<ReceiveStockHandler> logger)
    : ICommandHandler<ReceiveStockCommand, Result<InventoryItemResult>>
{
    public async Task<Result<InventoryItemResult>> HandleAsync(ReceiveStockCommand command, CancellationToken ct)
    {
        var item = await db.InventoryItems.SingleOrDefaultAsync(i => i.ProductId == command.ProductId, ct);
        if (item is null)
        {
            return DomainErrors.InventoryItem.NotFound(command.ProductId);
        }

        var receiveResult = item.Receive(command.Request.Quantity);
        if (receiveResult.IsFailure)
        {
            return receiveResult.Error;
        }

        await db.SaveChangesAsync(ct);

        logger.LogInformation("Received {Qty} units for product {ProductId}. New OnHand: {OnHand}",
            command.Request.Quantity, command.ProductId, item.OnHand);

        return item.ToResult();
    }
}
