using Catalog.Domain;
using Catalog.Domain.Repositories;
using Shared.Contracts;
using Shared.Core.CQRS;

namespace Catalog.Application.Products.Queries;

public record CheckStockQuery(IReadOnlyList<StockCheckItem> Items)
    : IQuery<StockCheckResponse>;

public sealed class CheckStockHandler(IProductRepository productRepo)
    : IQueryHandler<CheckStockQuery, StockCheckResponse>
{
    public async Task<StockCheckResponse> HandleAsync(CheckStockQuery query, CancellationToken ct)
    {
        var productIds = query.Items.Select(i => i.ProductId).ToList();
        var products = await productRepo.GetByIdsAsync(productIds, ct);
        var productMap = products.ToDictionary(p => p.Id);

        var failures = new List<StockCheckFailure>();

        foreach (var item in query.Items)
        {
            if (!productMap.TryGetValue(item.ProductId, out var product))
            {
                failures.Add(new StockCheckFailure(item.ProductId, $"Product {item.ProductId} not found"));
                continue;
            }

            if (product.StockQuantity < item.Quantity)
            {
                failures.Add(new StockCheckFailure(
                    item.ProductId,
                    DomainErrors.Product.InsufficientStock(product.Name, product.StockQuantity, item.Quantity).Message));
            }
        }

        return new StockCheckResponse(failures.Count == 0, failures);
    }
}
