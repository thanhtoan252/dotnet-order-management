using Catalog.Api.Common.ApiVersioning;
using Catalog.Api.Endpoints.Products.V1.Mappers;
using Catalog.Api.Endpoints.Products.V1.Validators;
using Catalog.Api.Extensions;
using Catalog.Application.Abstractions;
using Catalog.Application.Exceptions;
using Catalog.Application.Products.Commands;
using Catalog.Application.Products.Queries;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Shared.Core.CQRS;
using Shared.Web.Extensions;
using CmdDto = Catalog.Api.Endpoints.Products.V1.Commands.DTOs;

namespace Catalog.Api.Endpoints.Products.V1;

public static class ProductEndpoints
{
    public static IEndpointRouteBuilder MapProductEndpoints(this IEndpointRouteBuilder app)
    {
        var versionSet = app.NewCatalogApiVersionSet();

        var group = app.MapGroup("/api/products")
            .WithTags("Products")
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(CatalogApiVersions.V1)
            .RequireAuthorization();

        group.MapGet("/", GetProductsAsync)
            .WithName("GetProducts")
            .WithSummary("Get all available products");

        group.MapPost("/", CreateProductAsync)
            .WithName("CreateProduct")
            .WithSummary("Create a new product")
            .RequireAuthorization("product:create");

        group.MapPost("/import", ImportProductsAsync)
            .WithName("ImportProducts")
            .WithSummary("Bulk import products from an .xlsx file")
            .RequireAuthorization("product:create")
            .DisableAntiforgery()
            .Accepts<IFormFile>("multipart/form-data")
            .WithMetadata(new RequestSizeLimitAttribute(ProductImportFileValidator.MaxFileSizeBytes));

        group.MapPut("/{id:guid}", UpdateProductAsync)
            .WithName("UpdateProduct")
            .WithSummary("Update product name or price")
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
        var query = new GetAllProductsQuery(page, pageSize);
        var products = await dispatcher.QueryAsync(query, ct);

        return Results.Ok(products.Select(p => p.ToQueryDto()));
    }

    private static async Task<IResult> CreateProductAsync(CmdDto.CreateProductRequest request,
        IValidator<CmdDto.CreateProductRequest> validator, IDispatcher dispatcher, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
        {
            return Results.ValidationProblem(validation.ToDictionary());
        }

        var command = new CreateProductCommand(request.ToInput());
        var result = await dispatcher.SendAsync(command, ct);

        return result.IsSuccess
            ? Results.Created($"/api/products/{result.Value.Id}", result.Value.ToCommandDto())
            : Results.Problem(result.Error.ToProblemDetails());
    }

    private static async Task<IResult> ImportProductsAsync(
        IFormFile file,
        IValidator<IFormFile> fileValidator,
        IProductExcelParser parser,
        IDispatcher dispatcher,
        CancellationToken ct)
    {
        // 1. File gate
        var fileValidation = await fileValidator.ValidateAsync(file, ct);
        if (!fileValidation.IsValid)
        {
            return fileValidation.Errors.Any(e => e.ErrorCode == ProductImportFileValidator.PayloadTooLargeErrorCode)
                ? Results.StatusCode(StatusCodes.Status413PayloadTooLarge)
                : Results.ValidationProblem(fileValidation.ToDictionary());
        }

        // 2. Parse
        ProductExcelParseResult parseResult;
        try
        {
            await using var stream = file.OpenReadStream();
            parseResult = parser.Parse(stream);
        }
        catch (ProductExcelParseException ex)
        {
            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                ["file"] = [ex.Message]
            });
        }

        if (!parseResult.IsValid)
        {
            return Results.ValidationProblem(parseResult.Errors);
        }

        // 3. Dispatch all-or-nothing insert
        var command = new ImportProductsCommand(parseResult.Rows);
        var result = await dispatcher.SendAsync(command, ct);

        return result.IsSuccess
            ? Results.Ok(result.Value.ToDto())
            : Results.Problem(result.Error.ToProblemDetails());
    }

    private static async Task<IResult> UpdateProductAsync(Guid id, CmdDto.UpdateProductRequest request,
        IValidator<CmdDto.UpdateProductRequest> validator, IDispatcher dispatcher, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
        {
            return Results.ValidationProblem(validation.ToDictionary());
        }

        var command = new UpdateProductCommand(id, request.ToInput());
        var result = await dispatcher.SendAsync(command, ct);

        return result.IsSuccess
            ? Results.Ok(result.Value.ToCommandDto())
            : Results.Problem(result.Error.ToProblemDetails());
    }

    private static async Task<IResult> DeleteProductAsync(Guid id, IDispatcher dispatcher, CancellationToken ct)
    {
        var command = new DeleteProductCommand(id);
        var result = await dispatcher.SendAsync(command, ct);

        return result.IsSuccess
            ? Results.NoContent()
            : Results.Problem(result.Error.ToProblemDetails());
    }

}
