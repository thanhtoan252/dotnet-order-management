using Inventory.Domain.Entities;
using Inventory.Domain.Repositories;
using Inventory.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Infrastructure.Repositories;

public class InventoryRepository(InventoryDbContext db) : IInventoryRepository
{
    public async Task<InventoryItem?> GetByProductIdAsync(Guid productId, CancellationToken ct = default)
    {
        return await db.InventoryItems.SingleOrDefaultAsync(i => i.ProductId == productId, ct);
    }

    public async Task<IReadOnlyList<InventoryItem>> GetByProductIdsAsync(IEnumerable<Guid> productIds, CancellationToken ct = default)
    {
        var idList = productIds.ToList();
        return await db.InventoryItems
            .Where(i => idList.Contains(i.ProductId))
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<InventoryItem>> GetAllAsync(int page, int pageSize, CancellationToken ct = default)
    {
        return await db.InventoryItems
            .OrderBy(i => i.Sku)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking()
            .ToListAsync(ct);
    }

    public async Task<bool> ExistsForProductAsync(Guid productId, CancellationToken ct = default)
    {
        return await db.InventoryItems.AnyAsync(i => i.ProductId == productId, ct);
    }

    public void Add(InventoryItem item)
    {
        db.InventoryItems.Add(item);
    }

    public void Update(InventoryItem item)
    {
        db.InventoryItems.Update(item);
    }

    public void Remove(InventoryItem item)
    {
        db.InventoryItems.Remove(item);
    }
}
