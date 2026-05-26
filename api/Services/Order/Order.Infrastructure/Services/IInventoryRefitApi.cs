using Refit;
using Shared.Contracts;

namespace Order.Infrastructure.Services;

public interface IInventoryRefitApi
{
    [Post("/internal/inventory/availability")]
    Task<StockCheckResponse> CheckAvailabilityAsync([Body] StockCheckRequest request, CancellationToken ct);
}
