using Catalog.Application.Abstractions;
using Catalog.Application.Products.Mappers;
using Microsoft.EntityFrameworkCore;
using Shared.Core.CQRS;

namespace Catalog.Application.Products.Queries;

public record GetAllProductsQuery(int Page = 1, int PageSize = 100)
    : IQuery<IReadOnlyList<ProductResponse>>;

public class GetAllProductsHandler(ICatalogDbContext db)
    : IQueryHandler<GetAllProductsQuery, IReadOnlyList<ProductResponse>>
{
    public async Task<IReadOnlyList<ProductResponse>> HandleAsync(GetAllProductsQuery query, CancellationToken ct)
    {
        var products = await db.Products
            .OrderBy(p => p.Name)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .AsNoTracking()
            .ToListAsync(ct);

        return products.Select(p => p.ToQueryResponse()).ToList();
    }
}
