using OrderManagement.Domain.Common;
using OrderManagement.Domain.ValueObjects;

namespace OrderManagement.Domain.Entities;

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

    public ICollection<OrderItem> OrderItems { get; private set; } = [];

    public static Product Create(string name, string sku, Money price, int stock, string? description = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(sku);
        ArgumentNullException.ThrowIfNull(price);
        if (stock < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(stock));
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

    public Result Restock(int quantity)
    {
        if (quantity <= 0)
        {
            return DomainErrors.Product.InvalidQuantity;
        }
        StockQuantity += quantity;
        UpdatedAt = DateTime.UtcNow;
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
        UpdatedAt = DateTime.UtcNow;
        return Result.Success();
    }

    public Result RestoreStock(int quantity)
    {
        if (quantity <= 0)
        {
            return DomainErrors.Product.InvalidQuantity;
        }
        StockQuantity += quantity;
        UpdatedAt = DateTime.UtcNow;
        return Result.Success();
    }

    public Result UpdatePrice(Money newPrice)
    {
        ArgumentNullException.ThrowIfNull(newPrice);
        Price = newPrice;
        UpdatedAt = DateTime.UtcNow;
        return Result.Success();
    }

    public Result UpdateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return new Error("Product.InvalidName", "Product name cannot be empty.");
        }
        Name = name.Trim();
        UpdatedAt = DateTime.UtcNow;
        return Result.Success();
    }
}
