using Shared.Core.Domain;
using Shared.Core.ValueObjects;

namespace Order.Domain.Entities;

public class OrderItem : BaseEntity
{
    private OrderItem()
    {
    }

    public Guid OrderId { get; private set; }
    public Guid ProductId { get; private set; }
    public string ProductName { get; private set; } = null!;
    public Money UnitPrice { get; private set; } = null!;
    public int Quantity { get; private set; }
    public bool IsCancelled { get; private set; }

    public Money LineTotal => IsCancelled ? Money.Zero(UnitPrice.Currency) : UnitPrice * Quantity;

    // Navigation (no Product reference — separate service)
    public OrderAggregate Order { get; private set; } = null!;

    internal static OrderItem Create(Guid orderId, Guid productId, string productName, Money unitPrice, int quantity)
    {
        return new OrderItem
        {
            OrderId = orderId,
            ProductId = productId,
            ProductName = productName,
            UnitPrice = unitPrice,
            Quantity = quantity
        };
    }

    internal Result IncreaseQuantity(int additional, Money currentPrice)
    {
        if (additional <= 0)
        {
            return DomainErrors.Order.InvalidQuantity;
        }

        Quantity += additional;
        UnitPrice = currentPrice;

        return Result.Success();
    }

    internal void MarkCancelled()
    {
        IsCancelled = true;
    }
}
