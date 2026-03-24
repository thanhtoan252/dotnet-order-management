using OrderManagement.Domain.Common;
using OrderManagement.Domain.Events;
using OrderManagement.Domain.ValueObjects;

namespace OrderManagement.Domain.Entities;

public class Order : AggregateRoot
{
    private readonly List<OrderItem> _items = [];

    private Order()
    {
    }

    public string OrderNumber { get; private set; } = null!;
    public Guid CustomerId { get; private set; }
    public OrderStatus Status { get; private set; } = OrderStatus.Pending;
    public Money TotalAmount { get; private set; } = Money.Zero();
    public Address ShippingAddress { get; private set; } = null!;
    public string? Notes { get; private set; }
    public DateTime? ConfirmedAt { get; private set; }
    public DateTime? ShippedAt { get; private set; }
    public DateTime? DeliveredAt { get; private set; }
    public DateTime? CancelledAt { get; private set; }
    public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();

    public static Result<Order> Create(Guid customerId, Address shippingAddress, string? notes = null)
    {
        if (customerId == Guid.Empty)
        {
            return DomainErrors.Order.InvalidCustomer;
        }
        ArgumentNullException.ThrowIfNull(shippingAddress);

        var order = new Order
        {
            CustomerId = customerId,
            OrderNumber = GenerateOrderNumber(),
            ShippingAddress = shippingAddress,
            Notes = notes,
            Status = OrderStatus.Pending
        };

        order.AddDomainEvent(new OrderPlacedDomainEvent(order.Id, order.OrderNumber, customerId));
        return order;
    }

    public Result AddItem(Product product, int quantity)
    {
        if (Status != OrderStatus.Pending)
        {
            return DomainErrors.Order.InvalidState(Status.ToString(), "AddItem");
        }

        if (quantity <= 0)
        {
            return DomainErrors.Order.InvalidQuantity;
        }

        if (_items.Count > 0 && _items.First().UnitPrice.Currency != product.Price.Currency)
        {
            return DomainErrors.Order.CurrencyMismatch;
        }

        var deductResult = product.DeductStock(quantity);
        if (deductResult.IsFailure)
        {
            return deductResult;
        }

        var existingItem = _items.FirstOrDefault(i => i.ProductId == product.Id);
        if (existingItem is not null)
        {
            var increaseResult = existingItem.IncreaseQuantity(quantity, product.Price);
            if (increaseResult.IsFailure)
            {
                return increaseResult;
            }
        }
        else
        {
            _items.Add(OrderItem.Create(Id, product.Id, product.Name, product.Price, quantity));
        }

        RecalculateTotal();
        UpdatedAt = DateTime.UtcNow;
        return Result.Success();
    }

    public Result Confirm(string confirmedBy)
    {
        if (Status != OrderStatus.Pending)
        {
            return DomainErrors.Order.InvalidState(Status.ToString(), "Confirm");
        }

        if (_items.Count == 0)
        {
            return DomainErrors.Order.EmptyItems;
        }

        Status = OrderStatus.Confirmed;
        ConfirmedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = confirmedBy;

        AddDomainEvent(new OrderConfirmedDomainEvent(Id, OrderNumber));
        return Result.Success();
    }

    public Result Ship(string shippedBy)
    {
        if (Status is not (OrderStatus.Confirmed or OrderStatus.Processing))
        {
            return DomainErrors.Order.InvalidState(Status.ToString(), "Ship");
        }

        Status = OrderStatus.Shipped;
        ShippedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = shippedBy;

        AddDomainEvent(new OrderShippedDomainEvent(Id, OrderNumber));
        return Result.Success();
    }

    public Result MarkDelivered()
    {
        if (Status != OrderStatus.Shipped)
        {
            return DomainErrors.Order.InvalidState(Status.ToString(), "MarkDelivered");
        }

        Status = OrderStatus.Delivered;
        DeliveredAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new OrderDeliveredDomainEvent(Id, OrderNumber));
        return Result.Success();
    }

    public Result Cancel(string reason, string cancelledBy)
    {
        if (Status is OrderStatus.Shipped or OrderStatus.Delivered or OrderStatus.Cancelled)
        {
            return DomainErrors.Order.InvalidState(Status.ToString(), "Cancel");
        }

        Status = OrderStatus.Cancelled;
        Notes = string.IsNullOrWhiteSpace(Notes) ? reason : $"{Notes}\nCancellation: {reason}";
        CancelledAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = cancelledBy;

        AddDomainEvent(new OrderCancelledDomainEvent(Id, OrderNumber, reason));
        return Result.Success();
    }

    private void RecalculateTotal()
    {
        var activeItems = _items.Where(i => !i.IsCancelled).ToList();
        if (activeItems.Count == 0)
        {
            TotalAmount = Money.Zero(TotalAmount.Currency);
            return;
        }

        var currency = activeItems[0].LineTotal.Currency;
        TotalAmount = activeItems.Aggregate(Money.Zero(currency), (sum, i) => sum + i.LineTotal);
    }

    private static string GenerateOrderNumber()
    {
        return $"ORD-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..8].ToUpper()}";
    }
}