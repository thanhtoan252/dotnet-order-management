using Shared.Contracts;

namespace Order.Application.Services;

public interface IInventoryService
{
    Task<StockCheckResponse> CheckAvailabilityAsync(StockCheckRequest request, CancellationToken ct);
}
