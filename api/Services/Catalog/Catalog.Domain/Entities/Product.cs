using Shared.Core.Domain;
using Shared.Core.ValueObjects;

namespace Catalog.Domain.Entities;

public class Product : AggregateRoot
{
    private Product()
    {
    }

    public string Name { get; private set; } = null!;
    public string? Description { get; private set; }
    public Money Price { get; private set; } = null!;
    public int StockQuantity { get; private set; }
    public string SKU { get; private set; } = null!;

    public static Result<Product> Create(string name, string sku, Money price, int stock, string? description = null)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return DomainErrors.Product.InvalidName;
        }

        if (string.IsNullOrWhiteSpace(sku))
        {
            return DomainErrors.Product.InvalidSku;
        }

        if (price is null)
        {
            return DomainErrors.Product.InvalidPrice;
        }

        if (stock < 0)
        {
            return DomainErrors.Product.NegativeStock;
        }

        return new Product
        {
            Name = name.Trim(),
            SKU = sku.Trim().ToUpperInvariant(),
            Price = price,
            StockQuantity = stock,
            Description = description?.Trim()
        };
    }

    public Result AddStock(int quantity)
    {
        if (quantity <= 0)
        {
            return DomainErrors.Product.InvalidQuantity;
        }

        StockQuantity += quantity;

        return Result.Success();
    }

    public Result DeductStock(int quantity)
    {
        if (quantity <= 0)
        {
            return DomainErrors.Product.InvalidQuantity;
        }

        if (StockQuantity < quantity)
        {
            return DomainErrors.Product.InsufficientStock(Name, StockQuantity, quantity);
        }

        StockQuantity -= quantity;

        return Result.Success();
    }

    public Result UpdatePrice(Money newPrice)
    {
        ArgumentNullException.ThrowIfNull(newPrice);
        Price = newPrice;

        return Result.Success();
    }

    public Result UpdateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return DomainErrors.Product.InvalidName;
        }

        Name = name.Trim();

        return Result.Success();
    }
}
