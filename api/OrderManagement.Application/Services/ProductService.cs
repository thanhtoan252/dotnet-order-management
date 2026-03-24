using Microsoft.Extensions.Logging;
using OrderManagement.Application.Common.Interfaces;
using OrderManagement.Domain.Common;
using OrderManagement.Domain.Entities;
using OrderManagement.Domain.Repositories;
using OrderManagement.Domain.ValueObjects;

namespace OrderManagement.Application.Services;

public class ProductService(
    IProductRepository productRepo,
    IUnitOfWork uow,
    ILogger<ProductService> logger)
{
    public async Task<IReadOnlyList<Product>> GetAllProductsAsync(int page = 1, int pageSize = 100,
        CancellationToken ct = default)
    {
        return await productRepo.GetAllAsync(page, pageSize, ct);
    }

    public async Task<Product> CreateProductAsync(Product product, CancellationToken ct = default)
    {
        productRepo.Add(product);
        await uow.SaveChangesAsync(ct);
        logger.LogInformation("Product {Sku} created with Id {Id}.", product.SKU, product.Id);

        return product;
    }

    public async Task<Result<Product>> UpdateProductAsync(Guid id, string? name, Money? price, int? stockQuantity,
        CancellationToken ct = default)
    {
        var product = await productRepo.GetByIdAsync(id, ct);
        if (product is null)
        {
            return DomainErrors.Product.NotFound(id);
        }

        if (name is not null)
        {
            var nameResult = product.UpdateName(name);
            if (nameResult.IsFailure)
            {
                return nameResult.Error;
            }
        }

        if (price is not null)
        {
            product.UpdatePrice(price);
        }

        if (stockQuantity.HasValue)
        {
            var diff = stockQuantity.Value - product.StockQuantity;
            if (diff > 0)
            {
                var restockResult = product.Restock(diff);
                if (restockResult.IsFailure)
                {
                    return restockResult.Error;
                }
            }
            else if (diff < 0)
            {
                var deductResult = product.DeductStock(-diff);
                if (deductResult.IsFailure)
                {
                    return deductResult.Error;
                }
            }
        }

        productRepo.Update(product);
        await uow.SaveChangesAsync(ct);

        logger.LogInformation("Product {Id} updated.", id);

        return product;
    }

    public async Task<Result> DeleteProductAsync(Guid id, CancellationToken ct = default)
    {
        var product = await productRepo.GetByIdAsync(id, ct);
        if (product is null)
        {
            return DomainErrors.Product.NotFound(id);
        }

        product.IsDeleted = true;
        productRepo.Update(product);
        await uow.SaveChangesAsync(ct);

        logger.LogInformation("Product {Id} deleted.", id);

        return Result.Success();
    }
}
