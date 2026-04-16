using Catalog.Application.Products.Queries;
using Shared.Contracts;
using Shared.Core.CQRS;

namespace Catalog.Api.Endpoints;

public static class StockEndpoints
{
    public static IEndpointRouteBuilder MapStockEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/internal/products/stock-check", CheckStockAsync)
            .WithTags("Stock")
            .WithName("CheckStock")
            .WithSummary("Check stock availability for a list of products");

        return app;
    }

    private static async Task<IResult> CheckStockAsync(
        StockCheckRequest request, IDispatcher dispatcher, CancellationToken ct)
    {
        var result = await dispatcher.QueryAsync(new CheckStockQuery(request.Items), ct);

        return TypedResults.Ok(result);
    }
}
