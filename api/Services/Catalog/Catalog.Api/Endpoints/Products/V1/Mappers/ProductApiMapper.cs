using Catalog.Application.Products.Models;
using CmdDto = Catalog.Api.Endpoints.Products.V1.Commands.DTOs;
using QryDto = Catalog.Api.Endpoints.Products.V1.Queries.DTOs;

namespace Catalog.Api.Endpoints.Products.V1.Mappers;

internal static class ProductApiMapper
{
    internal static CreateProductInput ToInput(this CmdDto.CreateProductRequest request)
    {
        return new CreateProductInput(
            request.Name,
            request.Sku,
            request.Price,
            request.Currency,
            request.InitialStockQuantity,
            request.Description);
    }

    internal static UpdateProductInput ToInput(this CmdDto.UpdateProductRequest request)
    {
        return new UpdateProductInput(
            request.Name,
            request.Price,
            request.Currency);
    }

    internal static CmdDto.ProductResponse ToCommandDto(this ProductResult product)
    {
        return new CmdDto.ProductResponse
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Sku = product.Sku,
            Price = product.Price,
            Currency = product.Currency
        };
    }

    internal static QryDto.ProductResponse ToQueryDto(this ProductResult product)
    {
        return new QryDto.ProductResponse
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Sku = product.Sku,
            Price = product.Price,
            Currency = product.Currency
        };
    }

    internal static CmdDto.ImportProductsResponse ToDto(this ImportProductsResult result)
    {
        return new CmdDto.ImportProductsResponse
        {
            ImportedCount = result.ImportedCount,
            Products = result.Products.Select(p => p.ToCommandDto()).ToList()
        };
    }
}
