using Inventory.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Application.Abstractions;

public interface IInventoryDbContext
{
    DbSet<InventoryItem> InventoryItems { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
