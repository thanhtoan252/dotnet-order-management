using Order.Domain.Entities;

namespace Order.Application.Orders.Models;

public sealed record OrderResult(
    Guid Id,
    string OrderNumber,
    Guid CustomerId,
    OrderStatus Status,
    decimal TotalAmount,
    string Currency,
    AddressResult ShippingAddress,
    DateTimeOffset CreatedAt,
    IReadOnlyList<OrderItemResult> Items);
