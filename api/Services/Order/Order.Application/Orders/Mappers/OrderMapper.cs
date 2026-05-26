using Order.Application.Orders.Models;
using Order.Domain.Entities;
using Shared.Core.ValueObjects;

namespace Order.Application.Orders.Mappers;

internal static class OrderMapper
{
    internal static OrderResult ToResult(this OrderAggregate order)
    {
        return new OrderResult(
            order.Id,
            order.OrderNumber,
            order.CustomerId,
            order.Status,
            order.TotalAmount.Amount,
            order.TotalAmount.Currency,
            order.ShippingAddress.ToResult(),
            new DateTimeOffset(order.CreatedAt, TimeSpan.Zero),
            order.Items
                .Where(i => !i.IsCancelled)
                .Select(i => i.ToResult())
                .ToList());
    }

    internal static Address ToValueObject(this AddressInput address)
    {
        return Address.Create(
            address.Street,
            address.City,
            address.Province,
            address.ZipCode);
    }

    private static AddressResult ToResult(this Address address)
    {
        return new AddressResult(
            address.Street,
            address.City,
            address.Province,
            address.ZipCode);
    }

    private static OrderItemResult ToResult(this OrderItem item)
    {
        return new OrderItemResult(
            item.ProductId,
            item.ProductName,
            item.UnitPrice.Amount,
            item.UnitPrice.Currency,
            item.Quantity,
            item.LineTotal.Amount);
    }
}
