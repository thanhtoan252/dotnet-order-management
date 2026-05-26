namespace Order.Application.Orders.Models;

public sealed record OrderItemResult(
    Guid ProductId,
    string ProductName,
    decimal UnitPrice,
    string Currency,
    int Quantity,
    decimal LineTotal);
