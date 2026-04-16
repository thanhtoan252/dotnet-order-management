using Catalog.Api.Extensions;
using Catalog.Application.Products.Commands;
using Catalog.Application.Products.Queries;
using FluentValidation;
using Shared.Core.CQRS;

namespace Catalog.Api.Endpoints;

public static class ProductEndpoints
{
    public static IEndpointRouteBuilder MapProductEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/products")
            .WithTags("Products")
            .RequireAuthorization();

        group.MapGet("/", GetProductsAsync)
            .WithName("GetProducts")
            .WithSummary("Get all available products");

        group.MapPost("/", CreateProductAsync)
            .WithName("CreateProduct")
            .WithSummary("Create a new product")
            .RequireAuthorization("product:create");

        group.MapPut("/{id:guid}", UpdateProductAsync)
            .WithName("UpdateProduct")
            .WithSummary("Update product name, price, or stock")
            .RequireAuthorization("product:update");

        group.MapDelete("/{id:guid}", DeleteProductAsync)
            .WithName("DeleteProduct")
            .WithSummary("Delete a product")
            .RequireAuthorization("product:delete");

        return app;
    }

    private static async Task<IResult> GetProductsAsync(IDispatcher dispatcher, int page = 1, int pageSize = 100,
        CancellationToken ct = default)
    {
        var products = await dispatcher.QueryAsync(new GetAllProductsQuery(page, pageSize), ct);
        return TypedResults.Ok(products);
    }

    private static async Task<IResult> CreateProductAsync(CreateProductRequest request,
        IValidator<CreateProductRequest> validator, IDispatcher dispatcher, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
        {
            return Results.ValidationProblem(validation.ToDictionary());
        }

        var result = await dispatcher.SendAsync(new CreateProductCommand(request), ct);
        if (result.IsFailure)
        {
            return result.Error.ToProblem();
        }

        return TypedResults.Created($"/api/products/{result.Value.Id}", result.Value);
    }

    private static async Task<IResult> UpdateProductAsync(Guid id, UpdateProductRequest request,
        IValidator<UpdateProductRequest> validator, IDispatcher dispatcher, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
        {
            return Results.ValidationProblem(validation.ToDictionary());
        }

        var result = await dispatcher.SendAsync(new UpdateProductCommand(id, request), ct);
        if (result.IsFailure)
        {
            return result.Error.ToProblem();
        }

        return TypedResults.Ok(result.Value);
    }

    private static async Task<IResult> DeleteProductAsync(Guid id, IDispatcher dispatcher, CancellationToken ct)
    {
        var result = await dispatcher.SendAsync(new DeleteProductCommand(id), ct);
        if (result.IsFailure)
        {
            return result.Error.ToProblem();
        }

        return TypedResults.NoContent();
    }
}
