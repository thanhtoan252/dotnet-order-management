using Microsoft.EntityFrameworkCore;
using OrderManagement.Domain.Entities;
using OrderManagement.Domain.Repositories;
using OrderManagement.Infrastructure.Data;

namespace OrderManagement.Infrastructure.Repositories;

public class ProductRepository(OrderDbContext db) : IProductRepository
{
    public async Task<Product?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await db.Products.AsNoTracking().SingleOrDefaultAsync(p => p.Id == id, ct);
    }

    public async Task<IReadOnlyList<Product>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken ct = default)
    {
        var idList = ids.ToList();
        return await db.Products
            .Where(p => idList.Contains(p.Id))
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Product>> GetAllAsync(int page, int pageSize, CancellationToken ct = default)
    {
        return await db.Products
            .OrderBy(p => p.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking()
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Product>> GetBySkuPrefixAsync(string prefix, CancellationToken ct = default)
    {
        return await db.Products
            .Where(p => p.SKU.StartsWith(prefix))
            .ToListAsync(ct);
    }

    public void Add(Product product)
    {
        db.Products.Add(product);
    }

    public void Update(Product product)
    {
        db.Products.Update(product);
    }

    public void Remove(Product product)
    {
        db.Products.Remove(product);
    }
}