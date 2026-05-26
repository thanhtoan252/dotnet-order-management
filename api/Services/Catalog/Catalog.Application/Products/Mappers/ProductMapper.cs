using Catalog.Domain.Entities;
using Catalog.Application.Products.Models;

namespace Catalog.Application.Products.Mappers;

internal static class ProductMapper
{
    internal static ProductResult ToResult(this Product p)
    {
        return new ProductResult(
            p.Id,
            p.Name,
            p.Description,
            p.SKU,
            p.Price.Amount,
            p.Price.Currency);
    }
}
