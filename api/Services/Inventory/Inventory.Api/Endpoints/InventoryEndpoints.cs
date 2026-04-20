using FluentValidation;
using Inventory.Api.Extensions;
using Inventory.Application.Items.Commands;
using Inventory.Application.Items.Queries;
using Shared.Core.CQRS;

namespace Inventory.Api.Endpoints;

public static class InventoryEndpoints
{
    public static IEndpointRouteBuilder MapInventoryEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/inventory")
            .WithTags("Inventory")
            .RequireAuthorization();

        group.MapGet("/", GetAllAsync)
            .WithName("GetInventory")
            .WithSummary("List inventory items");

        group.MapGet("/{productId:guid}", GetByProductIdAsync)
            .WithName("GetInventoryByProductId")
            .WithSummary("Get inventory item for a single product");

        group.MapPost("/", CreateAsync)
            .WithName("CreateInventoryItem")
            .WithSummary("Create an inventory item for a product")
            .RequireAuthorization("inventory:adjust");

        group.MapPost("/{productId:guid}/receive", ReceiveAsync)
            .WithName("ReceiveStock")
            .WithSummary("Add quantity to on-hand stock")
            .RequireAuthorization("inventory:adjust");

        group.MapPost("/{productId:guid}/adjust", AdjustAsync)
            .WithName("AdjustStock")
            .WithSummary("Set on-hand stock to an absolute quantity")
            .RequireAuthorization("inventory:adjust");

        return app;
    }

    private static async Task<IResult> GetAllAsync(IDispatcher dispatcher, int page = 1, int pageSize = 100,
        CancellationToken ct = default)
    {
        var items = await dispatcher.QueryAsync(new GetAllInventoryQuery(page, pageSize), ct);
        return TypedResults.Ok(items);
    }

    private static async Task<IResult> GetByProductIdAsync(Guid productId, IDispatcher dispatcher, CancellationToken ct)
    {
        var item = await dispatcher.QueryAsync(new GetInventoryByProductIdQuery(productId), ct);
        return item is null ? TypedResults.NotFound() : TypedResults.Ok(item);
    }

    private static async Task<IResult> CreateAsync(CreateInventoryItemRequest request,
        IValidator<CreateInventoryItemRequest> validator, IDispatcher dispatcher, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
        {
            return Results.ValidationProblem(validation.ToDictionary());
        }

        var result = await dispatcher.SendAsync(new CreateInventoryItemCommand(request), ct);
        if (result.IsFailure)
        {
            return result.Error.ToProblem();
        }

        return TypedResults.Created($"/api/inventory/{result.Value.ProductId}", result.Value);
    }

    private static async Task<IResult> ReceiveAsync(Guid productId, ReceiveStockRequest request,
        IValidator<ReceiveStockRequest> validator, IDispatcher dispatcher, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
        {
            return Results.ValidationProblem(validation.ToDictionary());
        }

        var result = await dispatcher.SendAsync(new ReceiveStockCommand(productId, request), ct);
        if (result.IsFailure)
        {
            return result.Error.ToProblem();
        }

        return TypedResults.Ok(result.Value);
    }

    private static async Task<IResult> AdjustAsync(Guid productId, AdjustStockRequest request,
        IValidator<AdjustStockRequest> validator, IDispatcher dispatcher, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
        {
            return Results.ValidationProblem(validation.ToDictionary());
        }

        var result = await dispatcher.SendAsync(new AdjustStockCommand(productId, request), ct);
        if (result.IsFailure)
        {
            return result.Error.ToProblem();
        }

        return TypedResults.Ok(result.Value);
    }
}
