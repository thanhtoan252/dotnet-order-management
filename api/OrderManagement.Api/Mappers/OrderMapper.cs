using OrderManagement.Api.Helpers;
using OrderManagement.Domain.Entities;
using DTOs = OrderManagement.Application.Orders.Commands;

namespace OrderManagement.Api.Mappers;

internal static class OrderMapper
{
    internal static DTOs.OrderResponse ToResponse(this Order order)
    {
        return new DTOs.OrderResponse
        {
            Id = order.Id,
            OrderNumber = order.OrderNumber,
            CustomerId = order.CustomerId,
            Status = order.Status.MapTo<DTOs.OrderStatus>(),
            TotalAmount = order.TotalAmount.Amount,
            Currency = order.TotalAmount.Currency,
            ShippingAddress = new DTOs.AddressDto
            {
                Street = order.ShippingAddress.Street,
                City = order.ShippingAddress.City,
                Province = order.ShippingAddress.Province,
                ZipCode = order.ShippingAddress.ZipCode
            },
            CreatedAt = new DateTimeOffset(order.CreatedAt, TimeSpan.Zero),
            Items = order.Items
                .Where(i => !i.IsCancelled)
                .Select(i => new DTOs.OrderItemResponse
                {
                    ProductId = i.ProductId,
                    ProductName = i.ProductName,
                    UnitPrice = i.UnitPrice.Amount,
                    Currency = i.UnitPrice.Currency,
                    Quantity = i.Quantity,
                    LineTotal = i.LineTotal.Amount
                })
                .ToList()
        };
    }
}