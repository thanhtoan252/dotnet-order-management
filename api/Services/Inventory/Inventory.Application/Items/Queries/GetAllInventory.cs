using Inventory.Application.Abstractions;
using Inventory.Application.Items.Mappers;
using Microsoft.EntityFrameworkCore;
using Shared.Core.CQRS;

namespace Inventory.Application.Items.Queries;

public record GetAllInventoryQuery(int Page = 1, int PageSize = 100)
    : IQuery<IReadOnlyList<InventoryItemResponse>>;

public class GetAllInventoryHandler(IInventoryDbContext db)
    : IQueryHandler<GetAllInventoryQuery, IReadOnlyList<InventoryItemResponse>>
{
    public async Task<IReadOnlyList<InventoryItemResponse>> HandleAsync(GetAllInventoryQuery query, CancellationToken ct)
    {
        var items = await db.InventoryItems
            .OrderBy(i => i.Sku)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .AsNoTracking()
            .ToListAsync(ct);

        return items.Select(i => i.ToQueryResponse()).ToList();
    }
}
