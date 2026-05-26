namespace Catalog.Application.Products.Models;

public sealed record CreateProductInput(
    string Name,
    string Sku,
    decimal Price,
    string Currency,
    int? InitialStockQuantity,
    string? Description);
