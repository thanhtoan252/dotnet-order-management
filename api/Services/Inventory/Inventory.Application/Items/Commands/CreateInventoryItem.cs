using Inventory.Application.Items.Mappers;
using Inventory.Domain;
using Inventory.Domain.Entities;
using Inventory.Domain.Repositories;
using Microsoft.Extensions.Logging;
using Shared.Core.CQRS;
using Shared.Core.Domain;

namespace Inventory.Application.Items.Commands;

public record CreateInventoryItemCommand(CreateInventoryItemRequest Request)
    : ICommand<Result<InventoryItemResponse>>;

public class CreateInventoryItemHandler(IInventoryRepository repo, IUnitOfWork uow, ILogger<CreateInventoryItemHandler> logger)
    : ICommandHandler<CreateInventoryItemCommand, Result<InventoryItemResponse>>
{
    public async Task<Result<InventoryItemResponse>> HandleAsync(CreateInventoryItemCommand command, CancellationToken ct)
    {
        var request = command.Request;

        if (await repo.ExistsForProductAsync(request.ProductId, ct))
        {
            return DomainErrors.InventoryItem.AlreadyExists(request.ProductId);
        }

        var createResult = InventoryItem.Create( request.ProductId, request.Sku, request.ProductName, request.InitialQuantity);

        if (createResult.IsFailure)
        {
            return createResult.Error;
        }

        var item = createResult.Value;
        repo.Add(item);
        await uow.SaveChangesAsync(ct);

        logger.LogInformation("InventoryItem for product {ProductId} (sku {Sku}) created with initial quantity {Qty}",
            item.ProductId, item.Sku, item.OnHand);

        return item.ToCommandResponse();
    }
}
