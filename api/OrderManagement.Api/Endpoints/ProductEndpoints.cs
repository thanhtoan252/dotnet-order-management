using FluentValidation;
using OrderManagement.Api.Extensions;
using OrderManagement.Api.Mappers;
using OrderManagement.Application.Products;
using OrderManagement.Application.Products.Commands;
using OrderManagement.Application.Services;
using OrderManagement.Domain.Entities;
using OrderManagement.Domain.ValueObjects;

namespace OrderManagement.Api.Endpoints;

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

    private static async Task<IResult> GetProductsAsync(ProductService svc, int page = 1, int pageSize = 100,
        CancellationToken ct = default)
    {
        var products = await svc.GetAllProductsAsync(page, pageSize, ct);

        return TypedResults.Ok(products.Select(p => p.ToResponse()).ToList());
    }

    private static async Task<IResult> CreateProductAsync(CreateProductRequest request,
        IValidator<CreateProductRequest> validator, ProductService svc, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
        {
            return Results.ValidationProblem(validation.ToDictionary());
        }

        var price = Money.Create(request.Price, request.Currency);
        var product = Product.Create(request.Name, request.Sku, price, request.StockQuantity, request.Description);
        var created = await svc.CreateProductAsync(product, ct);

        return TypedResults.Created($"/api/products/{created.Id}", created.ToResponse());
    }

    private static async Task<IResult> UpdateProductAsync(Guid id, UpdateProductRequest request,
        IValidator<UpdateProductRequest> validator, ProductService svc, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
        {
            return Results.ValidationProblem(validation.ToDictionary());
        }

        var price = request.Price.HasValue
            ? Money.Create(request.Price.Value, request.Currency)
            : null;

        var result = await svc.UpdateProductAsync(id, request.Name, price, request.StockQuantity, ct);
        if (result.IsFailure)
        {
            return result.Error.ToProblem();
        }

        return TypedResults.Ok(result.Value.ToResponse());
    }

    private static async Task<IResult> DeleteProductAsync(Guid id, ProductService svc, CancellationToken ct)
    {
        var result = await svc.DeleteProductAsync(id, ct);
        if (result.IsFailure)
        {
            return result.Error.ToProblem();
        }

        return TypedResults.NoContent();
    }
}
