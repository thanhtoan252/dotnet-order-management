using Order.Application.Services;
using Shared.Contracts;

namespace Order.Infrastructure.Services;

internal sealed class InventoryServiceClient(IInventoryRefitApi api) : IInventoryService
{
    public Task<StockCheckResponse> CheckAvailabilityAsync(StockCheckRequest request, CancellationToken ct)
        => api.CheckAvailabilityAsync(request, ct);
}
