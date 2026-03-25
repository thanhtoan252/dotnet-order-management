using OrderManagement.Domain.Entities;
using CmdDto = OrderManagement.Application.Products.Commands;
using QryDto = OrderManagement.Application.Products.Queries;

namespace OrderManagement.Application.Products.Mappers;

internal static class ProductMapper
{
    internal static CmdDto.ProductResponse ToCommandResponse(this Product p)
    {
        return new CmdDto.ProductResponse
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

    internal static QryDto.ProductResponse ToQueryResponse(this Product p)
    {
        return new QryDto.ProductResponse
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
