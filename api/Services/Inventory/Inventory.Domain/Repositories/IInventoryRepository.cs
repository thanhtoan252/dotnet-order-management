using Inventory.Domain.Entities;

namespace Inventory.Domain.Repositories;

public interface IInventoryRepository
{
    Task<InventoryItem?> GetByProductIdAsync(Guid productId, CancellationToken ct = default);
    Task<IReadOnlyList<InventoryItem>> GetByProductIdsAsync(IEnumerable<Guid> productIds, CancellationToken ct = default);
    Task<IReadOnlyList<InventoryItem>> GetAllAsync(int page, int pageSize, CancellationToken ct = default);
    Task<bool> ExistsForProductAsync(Guid productId, CancellationToken ct = default);
    void Add(InventoryItem item);
    void Update(InventoryItem item);
    void Remove(InventoryItem item);
}
