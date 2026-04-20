using Shared.Core.Domain;

namespace Inventory.Domain.Entities;

public class InventoryItem : AggregateRoot
{
    private InventoryItem()
    {
    }

    public Guid ProductId { get; private set; }
    public string Sku { get; private set; } = null!;
    public string ProductName { get; private set; } = null!;
    public int OnHand { get; private set; }
    public int Reserved { get; private set; }

    public int Available => OnHand - Reserved;

    public static Result<InventoryItem> Create(Guid productId, string sku, string productName, int initialQuantity)
    {
        if (string.IsNullOrWhiteSpace(sku))
        {
            return DomainErrors.InventoryItem.InvalidSku;
        }

        if (string.IsNullOrWhiteSpace(productName))
        {
            return DomainErrors.InventoryItem.InvalidProductName;
        }

        if (initialQuantity < 0)
        {
            return DomainErrors.InventoryItem.InvalidQuantity;
        }

        return new InventoryItem
        {
            Id = Guid.NewGuid(),
            ProductId = productId,
            Sku = sku.Trim().ToUpperInvariant(),
            ProductName = productName.Trim(),
            OnHand = initialQuantity,
            Reserved = 0
        };
    }

    public Result Receive(int quantity)
    {
        if (quantity <= 0)
        {
            return DomainErrors.InventoryItem.InvalidQuantity;
        }

        OnHand += quantity;

        return Result.Success();
    }

    public Result Reserve(int quantity)
    {
        if (quantity <= 0)
        {
            return DomainErrors.InventoryItem.InvalidQuantity;
        }

        if (Available < quantity)
        {
            return DomainErrors.InventoryItem.InsufficientStock(ProductName, Available, quantity);
        }

        Reserved += quantity;

        return Result.Success();
    }

    public Result Commit(int quantity)
    {
        if (quantity <= 0)
        {
            return DomainErrors.InventoryItem.InvalidQuantity;
        }

        if (Reserved < quantity)
        {
            return DomainErrors.InventoryItem.InsufficientReserved(ProductName, Reserved, quantity);
        }

        Reserved -= quantity;
        OnHand -= quantity;

        return Result.Success();
    }

    public Result Release(int quantity)
    {
        if (quantity <= 0)
        {
            return DomainErrors.InventoryItem.InvalidQuantity;
        }

        if (Reserved < quantity)
        {
            return DomainErrors.InventoryItem.InsufficientReserved(ProductName, Reserved, quantity);
        }

        Reserved -= quantity;

        return Result.Success();
    }

    public Result SetOnHand(int target)
    {
        if (target < 0)
        {
            return DomainErrors.InventoryItem.InvalidQuantity;
        }

        if (target < Reserved)
        {
            return DomainErrors.InventoryItem.InsufficientReserved(ProductName, Reserved, Reserved - target);
        }

        OnHand = target;

        return Result.Success();
    }

    public void RenameProduct(string newName)
    {
        if (string.IsNullOrWhiteSpace(newName))
        {
            return;
        }

        ProductName = newName.Trim();
    }
}
