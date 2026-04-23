using Inventory.Application.Abstractions;
using Inventory.Application.Items.Mappers;
using Microsoft.EntityFrameworkCore;
using Shared.Core.CQRS;

namespace Inventory.Application.Items.Queries;

public record GetInventoryByProductIdQuery(Guid ProductId)
    : IQuery<InventoryItemResponse?>;

public class GetInventoryByProductIdHandler(IInventoryDbContext db)
    : IQueryHandler<GetInventoryByProductIdQuery, InventoryItemResponse?>
{
    public async Task<InventoryItemResponse?> HandleAsync(GetInventoryByProductIdQuery query, CancellationToken ct)
    {
        var item = await db.InventoryItems.SingleOrDefaultAsync(i => i.ProductId == query.ProductId, ct);
        return item?.ToQueryResponse();
    }
}
