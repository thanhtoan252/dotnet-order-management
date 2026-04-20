using Inventory.Application.Items.Queries;
using Shared.Contracts;
using Shared.Core.CQRS;

namespace Inventory.Api.Endpoints;

public static class InventoryInternalEndpoints
{
    public static IEndpointRouteBuilder MapInventoryInternalEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/internal/inventory/availability", CheckAvailabilityAsync)
            .WithTags("InventoryInternal")
            .WithName("CheckAvailability")
            .WithSummary("Check stock availability for a list of products (service-to-service)");

        return app;
    }

    private static async Task<IResult> CheckAvailabilityAsync(
        StockCheckRequest request, IDispatcher dispatcher, CancellationToken ct)
    {
        var result = await dispatcher.QueryAsync(new CheckAvailabilityQuery(request.Items), ct);
        return TypedResults.Ok(result);
    }
}
