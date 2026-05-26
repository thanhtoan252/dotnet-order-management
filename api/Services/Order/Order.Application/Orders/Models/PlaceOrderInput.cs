namespace Order.Application.Orders.Models;

public sealed record PlaceOrderInput(
    Guid CustomerId,
    AddressInput ShippingAddress,
    IReadOnlyList<OrderLineInput> Lines,
    string? Notes);
