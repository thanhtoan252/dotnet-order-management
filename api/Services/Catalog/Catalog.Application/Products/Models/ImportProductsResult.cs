namespace Catalog.Application.Products.Models;

public sealed record ImportProductsResult(int ImportedCount, IReadOnlyList<ProductResult> Products);
