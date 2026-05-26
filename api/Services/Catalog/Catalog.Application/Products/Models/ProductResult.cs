namespace Catalog.Application.Products.Models;

public sealed record ProductResult(
    Guid Id,
    string Name,
    string? Description,
    string Sku,
    decimal Price,
    string Currency);
