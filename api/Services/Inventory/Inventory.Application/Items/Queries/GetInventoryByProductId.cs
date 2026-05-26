using Inventory.Application.Abstractions;
using Inventory.Application.Items.Mappers;
using Inventory.Application.Items.Models;
using Microsoft.EntityFrameworkCore;
using Shared.Core.CQRS;

namespace Inventory.Application.Items.Queries;

public record GetInventoryByProductIdQuery(Guid ProductId)
    : IQuery<InventoryItemResult?>;

public class GetInventoryByProductIdHandler(IInventoryDbContext db)
    : IQueryHandler<GetInventoryByProductIdQuery, InventoryItemResult?>
{
    public async Task<InventoryItemResult?> HandleAsync(GetInventoryByProductIdQuery query, CancellationToken ct)
    {
        var item = await db.InventoryItems.SingleOrDefaultAsync(i => i.ProductId == query.ProductId, ct);
        return item?.ToResult();
    }
}
