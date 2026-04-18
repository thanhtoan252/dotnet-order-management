using Refit;
using Shared.Contracts;

namespace Order.Application.Services;

public interface IInventoryService
{
    [Post("/internal/inventory/availability")]
    Task<StockCheckResponse> CheckAvailabilityAsync([Body] StockCheckRequest request, CancellationToken ct);
}
