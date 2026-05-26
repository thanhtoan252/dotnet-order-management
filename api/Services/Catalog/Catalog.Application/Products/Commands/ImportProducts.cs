using Catalog.Application.Products.Mappers;
using Catalog.Application.Products.Models;
using Catalog.Domain;
using Catalog.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared.Contracts;
using Shared.Contracts.IntegrationEvents;
using Shared.Core.CQRS;
using Shared.Core.Domain;
using Shared.Core.ValueObjects;
using Shared.Messaging.Abstractions;
using ICatalogDb = Catalog.Application.Abstractions.ICatalogDbContext;

namespace Catalog.Application.Products.Commands;

public sealed record ImportProductsRow(
    int RowNumber,
    string? Name,
    string? Sku,
    decimal? Price,
    string? Currency,
    string? Description,
    int? InitialStockQuantity);

public record ImportProductsCommand(IReadOnlyList<ImportProductsRow> Rows)
    : ICommand<Result<ImportProductsResult>>;

public sealed class ImportProductsHandler(
    ICatalogDb db,
    IEventBus eventBus,
    ILogger<ImportProductsHandler> logger)
    : ICommandHandler<ImportProductsCommand, Result<ImportProductsResult>>
{
    public async Task<Result<ImportProductsResult>> HandleAsync(ImportProductsCommand command, CancellationToken ct)
    {
        var skus = command.Rows
            .Where(r => !string.IsNullOrWhiteSpace(r.Sku))
            .Select(r => r.Sku!.Trim().ToUpperInvariant())
            .Distinct()
            .ToList();

        if (skus.Count > 0)
        {
            var existingSku = await db.Products
                .Where(p => skus.Contains(p.SKU))
                .Select(p => p.SKU)
                .FirstOrDefaultAsync(ct);

            if (existingSku is not null)
            {
                return DomainErrors.Product.SkuAlreadyExists(existingSku);
            }
        }

        var imported = new List<Product>(command.Rows.Count);

        foreach (var row in command.Rows)
        {
            var priceResult = Money.Create(row.Price!.Value, row.Currency!);
            if (priceResult.IsFailure)
            {
                return priceResult.Error;
            }

            var productResult = Product.Create(row.Name!, row.Sku!, priceResult.Value, row.Description);
            if (productResult.IsFailure)
            {
                return productResult.Error;
            }

            var product = productResult.Value;
            db.Products.Add(product);

            await eventBus.PublishAsync(
                new ProductCreatedIntegrationEvent(
                    Guid.NewGuid(),
                    DateTime.UtcNow,
                    product.Id,
                    product.SKU,
                    product.Name,
                    row.InitialStockQuantity ?? 0),
                Topics.ProductCreated,
                product.Id.ToString(),
                ct);

            imported.Add(product);
        }

        await db.SaveChangesAsync(ct);

        logger.LogInformation("Imported {Count} products in bulk.", imported.Count);

        return new ImportProductsResult(imported.Count, imported.Select(p => p.ToResult()).ToList());
    }
}
