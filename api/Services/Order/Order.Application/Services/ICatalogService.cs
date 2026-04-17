using Refit;
using Shared.Contracts;

namespace Order.Application.Services;

public interface ICatalogService
{
    [Post("/internal/products/stock-check")]
    Task<StockCheckResponse> CheckStockAsync([Body] StockCheckRequest request, CancellationToken ct);
}
