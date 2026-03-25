using OrderManagement.Application.Common.Interfaces;
using OrderManagement.Application.Products.Mappers;
using OrderManagement.Domain.Repositories;

namespace OrderManagement.Application.Products.Queries;

public record GetAllProductsQuery(int Page = 1, int PageSize = 100)
    : IQuery<IReadOnlyList<ProductResponse>>;

public class GetAllProductsHandler(IProductRepository productRepo)
    : IQueryHandler<GetAllProductsQuery, IReadOnlyList<ProductResponse>>
{
    public async Task<IReadOnlyList<ProductResponse>> HandleAsync(GetAllProductsQuery query, CancellationToken ct)
    {
        var products = await productRepo.GetAllAsync(query.Page, query.PageSize, ct);
        return products.Select(p => p.ToQueryResponse()).ToList();
    }
}
