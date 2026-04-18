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
    public string SKU { get; private set; } = null!;

    public static Result<Product> Create(string name, string sku, Money price, string? description = null)
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

        return new Product
        {
            Name = name.Trim(),
            SKU = sku.Trim().ToUpperInvariant(),
            Price = price,
            Description = description?.Trim()
        };
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
