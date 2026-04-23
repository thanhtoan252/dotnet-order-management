using Inventory.Application.Items.Mappers;
using Inventory.Domain.Repositories;
using Shared.Core.CQRS;

namespace Inventory.Application.Items.Queries;

public record GetAllInventoryQuery(int Page = 1, int PageSize = 100)
    : IQuery<IReadOnlyList<InventoryItemResponse>>;

public class GetAllInventoryHandler(IInventoryRepository repo)
    : IQueryHandler<GetAllInventoryQuery, IReadOnlyList<InventoryItemResponse>>
{
    public async Task<IReadOnlyList<InventoryItemResponse>> HandleAsync(GetAllInventoryQuery query, CancellationToken ct)
    {
        var items = await repo.GetAllAsync(query.Page, query.PageSize, ct);
        return items.Select(i => i.ToQueryResponse()).ToList();
    }
}
