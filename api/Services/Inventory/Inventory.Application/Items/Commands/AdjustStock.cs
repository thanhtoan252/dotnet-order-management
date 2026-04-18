using Inventory.Application.Items.Mappers;
using Inventory.Domain;
using Inventory.Domain.Repositories;
using Microsoft.Extensions.Logging;
using Shared.Core.CQRS;
using Shared.Core.Domain;

namespace Inventory.Application.Items.Commands;

public record AdjustStockCommand(Guid ProductId, AdjustStockRequest Request)
    : ICommand<Result<InventoryItemResponse>>;

public class AdjustStockHandler(
    IInventoryRepository repo,
    IUnitOfWork uow,
    ILogger<AdjustStockHandler> logger)
    : ICommandHandler<AdjustStockCommand, Result<InventoryItemResponse>>
{
    public async Task<Result<InventoryItemResponse>> HandleAsync(AdjustStockCommand command, CancellationToken ct)
    {
        var item = await repo.GetByProductIdAsync(command.ProductId, ct);
        if (item is null)
        {
            return DomainErrors.InventoryItem.NotFound(command.ProductId);
        }

        var setResult = item.SetOnHand(command.Request.OnHand);
        if (setResult.IsFailure)
        {
            return setResult.Error;
        }

        await uow.SaveChangesAsync(ct);

        logger.LogInformation("Adjusted stock for product {ProductId} to {OnHand}. Reason: {Reason}",
            command.ProductId, item.OnHand, command.Request.Reason ?? "(none)");

        return item.ToCommandResponse();
    }
}
