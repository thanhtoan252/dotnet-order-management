using OrderManagement.Application.Common.Helpers;
using OrderManagement.Domain.Entities;
using CmdDto = OrderManagement.Application.Orders.Commands;
using QryDto = OrderManagement.Application.Orders.Queries;

namespace OrderManagement.Application.Orders.Mappers;

internal static class OrderMapper
{
    internal static CmdDto.OrderResponse ToCommandResponse(this Order order)
    {
        return new CmdDto.OrderResponse
        {
            Id = order.Id,
            OrderNumber = order.OrderNumber,
            CustomerId = order.CustomerId,
            Status = order.Status.MapTo<CmdDto.OrderStatus>(),
            TotalAmount = order.TotalAmount.Amount,
            Currency = order.TotalAmount.Currency,
            ShippingAddress = new CmdDto.AddressDto
            {
                Street = order.ShippingAddress.Street,
                City = order.ShippingAddress.City,
                Province = order.ShippingAddress.Province,
                ZipCode = order.ShippingAddress.ZipCode
            },
            CreatedAt = new DateTimeOffset(order.CreatedAt, TimeSpan.Zero),
            Items = order.Items
                .Where(i => !i.IsCancelled)
                .Select(i => new CmdDto.OrderItemResponse
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

    internal static QryDto.OrderResponse ToQueryResponse(this Order order)
    {
        return new QryDto.OrderResponse
        {
            Id = order.Id,
            OrderNumber = order.OrderNumber,
            CustomerId = order.CustomerId,
            Status = order.Status.MapTo<QryDto.OrderStatus>(),
            TotalAmount = order.TotalAmount.Amount,
            Currency = order.TotalAmount.Currency,
            ShippingAddress = new QryDto.AddressDto
            {
                Street = order.ShippingAddress.Street,
                City = order.ShippingAddress.City,
                Province = order.ShippingAddress.Province,
                ZipCode = order.ShippingAddress.ZipCode
            },
            CreatedAt = new DateTimeOffset(order.CreatedAt, TimeSpan.Zero),
            Items = order.Items
                .Where(i => !i.IsCancelled)
                .Select(i => new QryDto.OrderItemResponse
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
