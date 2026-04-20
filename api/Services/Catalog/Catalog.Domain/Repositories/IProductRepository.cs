using Catalog.Domain.Entities;

namespace Catalog.Domain.Repositories;

public interface IProductRepository
{
    Task<Product?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<Product>> GetAllAsync(int page, int pageSize, CancellationToken ct = default);
    Task<IReadOnlyList<Product>> GetBySkuPrefixAsync(string prefix, CancellationToken ct = default);
    void Add(Product product);
    void Update(Product product);
    void Remove(Product product);
}
