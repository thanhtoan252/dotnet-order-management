using OrderManagement.Application.Products.Commands;
using OrderManagement.Domain.Entities;

namespace OrderManagement.Api.Mappers;

internal static class ProductMapper
{
    internal static ProductResponse ToResponse(this Product p)
    {
        return new ProductResponse
        {
            Id = p.Id,
            Name = p.Name,
            Sku = p.SKU,
            Price = p.Price.Amount,
            Currency = p.Price.Currency,
            StockQuantity = p.StockQuantity,
            Description = p.Description
        };
    }
}