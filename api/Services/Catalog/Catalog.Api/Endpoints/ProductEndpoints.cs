using Catalog.Api.Extensions;
using Catalog.Application.Abstractions;
using Catalog.Application.Products.Commands;
using Catalog.Application.Products.Queries;
using FluentValidation;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shared.Core.CQRS;

namespace Catalog.Api.Endpoints;

public static class ProductEndpoints
{
    private const long MaxFileSizeBytes = 5 * 1024 * 1024;

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

        group.MapPost("/import", ImportProductsAsync)
            .WithName("ImportProducts")
            .WithSummary("Bulk import products from an .xlsx file")
            .RequireAuthorization("product:create")
            .DisableAntiforgery()
            .Accepts<IFormFile>("multipart/form-data")
            .WithMetadata(new RequestSizeLimitAttribute(MaxFileSizeBytes));

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

    private static async Task<IResult> ImportProductsAsync(
        IFormFile file,
        IProductExcelParser parser,
        IValidator<ImportProductsRow> rowValidator,
        ICatalogDbContext db,
        IDispatcher dispatcher,
        CancellationToken ct)
    {
        // 1. File gate
        var gateResult = ValidateFile(file);
        if (gateResult is not null)
        {
            return gateResult;
        }

        // 2. Parse
        IReadOnlyList<ImportProductsRow> rows;
        try
        {
            await using var stream = file.OpenReadStream();
            rows = parser.Parse(stream);
        }
        catch (ProductExcelParseException ex)
        {
            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                ["file"] = [ex.Message]
            });
        }

        if (rows.Count == 0)
        {
            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                ["file"] = ["No data rows found in the file."]
            });
        }

        var errors = new Dictionary<string, string[]>();

        // 3. Per-row validation
        foreach (var row in rows)
        {
            var v = await rowValidator.ValidateAsync(row, ct);
            if (!v.IsValid)
            {
                foreach (var failure in v.Errors)
                {
                    var key = $"row[{row.RowNumber}].{ToCamelCase(failure.PropertyName)}";
                    errors[key] = [failure.ErrorMessage];
                }
            }
        }

        // 4. In-file SKU duplicate detection
        var skuGroups = rows
            .Where(r => !string.IsNullOrWhiteSpace(r.Sku))
            .GroupBy(r => r.Sku!.Trim().ToUpperInvariant())
            .Where(g => g.Count() > 1);

        foreach (var group in skuGroups)
        {
            var firstRowNumber = group.First().RowNumber;
            foreach (var duplicate in group.Skip(1))
            {
                var key = $"row[{duplicate.RowNumber}].sku";
                errors[key] = [$"Duplicate SKU '{group.Key}' in file (first seen at row {firstRowNumber})."];
            }
        }

        // 5. DB SKU conflict check
        var validSkus = rows
            .Where(r => !string.IsNullOrWhiteSpace(r.Sku))
            .Select(r => r.Sku!.Trim().ToUpperInvariant())
            .Distinct()
            .ToList();

        if (validSkus.Count > 0)
        {
            var existingSkus = await db.Products
                .Where(p => validSkus.Contains(p.SKU))
                .Select(p => p.SKU)
                .ToListAsync(ct);

            if (existingSkus.Count > 0)
            {
                var existingSet = new HashSet<string>(existingSkus, StringComparer.OrdinalIgnoreCase);
                foreach (var row in rows.Where(r => !string.IsNullOrWhiteSpace(r.Sku)
                    && existingSet.Contains(r.Sku!.Trim().ToUpperInvariant())))
                {
                    var key = $"row[{row.RowNumber}].sku";
                    errors[key] = [$"SKU '{row.Sku}' already exists."];
                }
            }
        }

        if (errors.Count > 0)
        {
            return Results.ValidationProblem(errors);
        }

        // 6. Dispatch all-or-nothing insert
        var result = await dispatcher.SendAsync(new ImportProductsCommand(rows), ct);
        if (result.IsFailure)
        {
            return result.Error.ToProblem();
        }

        return TypedResults.Ok(result.Value);
    }

    private static IResult? ValidateFile(IFormFile file)
    {
        if (file is null || file.Length == 0)
        {
            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                ["file"] = ["No file was uploaded."]
            });
        }

        if (file.Length > MaxFileSizeBytes)
        {
            return Results.StatusCode(StatusCodes.Status413PayloadTooLarge);
        }

        var extension = Path.GetExtension(file.FileName);
        if (!string.Equals(extension, ".xlsx", StringComparison.OrdinalIgnoreCase))
        {
            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                ["file"] = ["Only .xlsx files are accepted."]
            });
        }

        var allowedContentTypes = new[]
        {
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "application/octet-stream"
        };

        if (!allowedContentTypes.Contains(file.ContentType, StringComparer.OrdinalIgnoreCase))
        {
            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                ["file"] = ["Only .xlsx files are accepted."]
            });
        }

        // Zip magic bytes: PK (50 4B 03 04)
        using var peek = file.OpenReadStream();
        Span<byte> magic = stackalloc byte[4];
        var read = peek.Read(magic);
        if (read < 4 || magic[0] != 0x50 || magic[1] != 0x4B || magic[2] != 0x03 || magic[3] != 0x04)
        {
            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                ["file"] = ["Only .xlsx files are accepted."]
            });
        }

        return null;
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

    private static string ToCamelCase(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            return name;
        }

        return char.ToLowerInvariant(name[0]) + name[1..];
    }
}
