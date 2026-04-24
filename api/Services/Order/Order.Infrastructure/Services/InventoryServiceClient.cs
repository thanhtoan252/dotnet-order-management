using Order.Application.Services;
using Refit;
using Shared.Contracts;

namespace Order.Infrastructure.Services;

public interface IInventoryRefitApi
{
    [Post("/internal/inventory/availability")]
    Task<StockCheckResponse> CheckAvailabilityAsync([Body] StockCheckRequest request, CancellationToken ct);
}

internal sealed class InventoryServiceClient(IInventoryRefitApi api) : IInventoryService
{
    public Task<StockCheckResponse> CheckAvailabilityAsync(StockCheckRequest request, CancellationToken ct)
        => api.CheckAvailabilityAsync(request, ct);
}
