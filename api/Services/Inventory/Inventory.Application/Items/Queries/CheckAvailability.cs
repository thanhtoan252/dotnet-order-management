using Inventory.Domain;
using Inventory.Domain.Repositories;
using Shared.Contracts;
using Shared.Core.CQRS;

namespace Inventory.Application.Items.Queries;

public record CheckAvailabilityQuery(IReadOnlyList<StockCheckItem> Items)
    : IQuery<StockCheckResponse>;

public sealed class CheckAvailabilityHandler(IInventoryRepository repo)
    : IQueryHandler<CheckAvailabilityQuery, StockCheckResponse>
{
    public async Task<StockCheckResponse> HandleAsync(CheckAvailabilityQuery query, CancellationToken ct)
    {
        var productIds = query.Items.Select(i => i.ProductId).ToList();
        var items = await repo.GetByProductIdsAsync(productIds, ct);
        var byProduct = items.ToDictionary(i => i.ProductId);

        var failures = new List<StockCheckFailure>();

        foreach (var requested in query.Items)
        {
            if (!byProduct.TryGetValue(requested.ProductId, out var item))
            {
                failures.Add(new StockCheckFailure(requested.ProductId,
                    DomainErrors.InventoryItem.NotFound(requested.ProductId).Message));
                continue;
            }

            if (item.Available < requested.Quantity)
            {
                failures.Add(new StockCheckFailure(
                    requested.ProductId,
                    DomainErrors.InventoryItem.InsufficientStock(item.ProductName, item.Available, requested.Quantity).Message));
            }
        }

        return new StockCheckResponse(failures.Count == 0, failures);
    }
}
