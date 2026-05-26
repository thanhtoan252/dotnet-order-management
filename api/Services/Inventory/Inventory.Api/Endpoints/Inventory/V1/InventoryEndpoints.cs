using Inventory.Api.ApiVersioning;
using Inventory.Api.Endpoints.Inventory.V1.Mappers;
using Inventory.Api.Extensions;
using FluentValidation;
using Inventory.Application.Items.Commands;
using Inventory.Application.Items.Queries;
using Shared.Core.CQRS;
using Shared.Web.Extensions;
using CmdDto = Inventory.Api.Endpoints.Inventory.V1.Commands.DTOs;

namespace Inventory.Api.Endpoints.Inventory.V1;

public static class InventoryEndpoints
{
    public static IEndpointRouteBuilder MapInventoryEndpoints(this IEndpointRouteBuilder app)
    {
        var versionSet = app.NewInventoryApiVersionSet();

        var group = app.MapGroup("/api/inventory")
            .WithTags("Inventory")
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(InventoryApiVersions.V1)
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
        return Results.Ok(items.Select(i => i.ToQueryDto()));
    }

    private static async Task<IResult> GetByProductIdAsync(Guid productId, IDispatcher dispatcher, CancellationToken ct)
    {
        var item = await dispatcher.QueryAsync(new GetInventoryByProductIdQuery(productId), ct);
        return item is null ? TypedResults.NotFound() : Results.Ok(item.ToQueryDto());
    }

    private static async Task<IResult> CreateAsync(CmdDto.CreateInventoryItemRequest request,
        IValidator<CmdDto.CreateInventoryItemRequest> validator, IDispatcher dispatcher, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
        {
            return Results.ValidationProblem(validation.ToDictionary());
        }

        var command = new CreateInventoryItemCommand(request.ToInput());
        var result = await dispatcher.SendAsync(command, ct);

        return result.IsSuccess
            ? Results.Created($"/api/inventory/{result.Value.ProductId}", result.Value.ToCommandDto())
            : Results.Problem(result.Error.ToProblemDetails());
    }

    private static async Task<IResult> ReceiveAsync(Guid productId, CmdDto.ReceiveStockRequest request,
        IValidator<CmdDto.ReceiveStockRequest> validator, IDispatcher dispatcher, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
        {
            return Results.ValidationProblem(validation.ToDictionary());
        }
        var command = new ReceiveStockCommand(productId, request.ToInput());
        var result = await dispatcher.SendAsync(command, ct);

        return result.IsSuccess
            ? Results.Ok(result.Value.ToCommandDto())
            : Results.Problem(result.Error.ToProblemDetails());
    }

    private static async Task<IResult> AdjustAsync(Guid productId, CmdDto.AdjustStockRequest request,
        IValidator<CmdDto.AdjustStockRequest> validator, IDispatcher dispatcher, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
        {
            return Results.ValidationProblem(validation.ToDictionary());
        }

        var command = new AdjustStockCommand(productId, request.ToInput());
        var result = await dispatcher.SendAsync(command, ct);

        return result.IsSuccess
            ? Results.Ok(result.Value.ToCommandDto())
            : Results.Problem(result.Error.ToProblemDetails());
    }
}
