using Catalog.Application.Abstractions;
using Catalog.Application.Products.Mappers;
using Catalog.Application.Products.Models;
using Microsoft.EntityFrameworkCore;
using Shared.Core.CQRS;

namespace Catalog.Application.Products.Queries;

public record GetAllProductsQuery(int Page = 1, int PageSize = 100)
    : IQuery<IReadOnlyList<ProductResult>>;

public class GetAllProductsHandler(ICatalogDbContext db)
    : IQueryHandler<GetAllProductsQuery, IReadOnlyList<ProductResult>>
{
    public async Task<IReadOnlyList<ProductResult>> HandleAsync(GetAllProductsQuery query, CancellationToken ct)
    {
        var products = await db.Products
            .OrderBy(p => p.Name)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .AsNoTracking()
            .ToListAsync(ct);

        return products.Select(p => p.ToResult()).ToList();
    }
}
