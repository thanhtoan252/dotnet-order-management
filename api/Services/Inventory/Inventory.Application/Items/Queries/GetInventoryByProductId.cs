using Inventory.Application.Items.Mappers;
using Inventory.Domain.Repositories;
using Shared.Core.CQRS;

namespace Inventory.Application.Items.Queries;

public record GetInventoryByProductIdQuery(Guid ProductId)
    : IQuery<InventoryItemResponse?>;

public class GetInventoryByProductIdHandler(IInventoryRepository repo)
    : IQueryHandler<GetInventoryByProductIdQuery, InventoryItemResponse?>
{
    public async Task<InventoryItemResponse?> HandleAsync(GetInventoryByProductIdQuery query, CancellationToken ct)
    {
        var item = await repo.GetByProductIdAsync(query.ProductId, ct);
        return item?.ToQueryResponse();
    }
}
