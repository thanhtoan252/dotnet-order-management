using Inventory.Application.Items.Mappers;
using Inventory.Domain;
using Inventory.Domain.Repositories;
using Microsoft.Extensions.Logging;
using Shared.Core.CQRS;
using Shared.Core.Domain;

namespace Inventory.Application.Items.Commands;

public record ReceiveStockCommand(Guid ProductId, ReceiveStockRequest Request)
    : ICommand<Result<InventoryItemResponse>>;

public class ReceiveStockHandler(IInventoryRepository repo, IUnitOfWork uow, ILogger<ReceiveStockHandler> logger)
    : ICommandHandler<ReceiveStockCommand, Result<InventoryItemResponse>>
{
    public async Task<Result<InventoryItemResponse>> HandleAsync(ReceiveStockCommand command, CancellationToken ct)
    {
        var item = await repo.GetByProductIdAsync(command.ProductId, ct);
        if (item is null)
        {
            return DomainErrors.InventoryItem.NotFound(command.ProductId);
        }

        var receiveResult = item.Receive(command.Request.Quantity);
        if (receiveResult.IsFailure)
        {
            return receiveResult.Error;
        }

        await uow.SaveChangesAsync(ct);

        logger.LogInformation("Received {Qty} units for product {ProductId}. New OnHand: {OnHand}",
            command.Request.Quantity, command.ProductId, item.OnHand);

        return item.ToCommandResponse();
    }
}
