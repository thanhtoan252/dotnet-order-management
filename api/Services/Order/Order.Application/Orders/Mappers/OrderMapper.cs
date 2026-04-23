using Order.Application.Common.Helpers;
using Order.Domain.Entities;
using Shared.Core.ValueObjects;
using System.Runtime.CompilerServices;
using CmdDto = Order.Application.Orders.Commands;
using QryDto = Order.Application.Orders.Queries;

[assembly: InternalsVisibleTo("Order.UnitTests")]

namespace Order.Application.Orders.Mappers;

internal static class OrderMapper
{
    internal static CmdDto.OrderResponse ToCommandResponse(this OrderAggregate order)
    {
        return new CmdDto.OrderResponse
        {
            Id = order.Id,
            OrderNumber = order.OrderNumber,
            CustomerId = order.CustomerId,
            Status = order.Status.MapTo<CmdDto.OrderStatus>(),
            TotalAmount = order.TotalAmount.Amount,
            Currency = order.TotalAmount.Currency,
            ShippingAddress = order.ShippingAddress.ToCommandDto(),
            CreatedAt = new DateTimeOffset(order.CreatedAt, TimeSpan.Zero),
            Items = order.Items
                .Where(i => !i.IsCancelled)
                .Select(i => i.ToCommandDto())
                .ToList()
        };
    }

    internal static QryDto.OrderResponse ToQueryResponse(this OrderAggregate order)
    {
        return new QryDto.OrderResponse
        {
            Id = order.Id,
            OrderNumber = order.OrderNumber,
            CustomerId = order.CustomerId,
            Status = order.Status.MapTo<QryDto.OrderStatus>(),
            TotalAmount = order.TotalAmount.Amount,
            Currency = order.TotalAmount.Currency,
            ShippingAddress = order.ShippingAddress.ToQueryDto(),
            CreatedAt = new DateTimeOffset(order.CreatedAt, TimeSpan.Zero),
            Items = order.Items
                .Where(i => !i.IsCancelled)
                .Select(i => i.ToQueryDto())
                .ToList()
        };
    }

    private static CmdDto.AddressDto ToCommandDto(this Address address)
    {
        return new CmdDto.AddressDto
        {
            Street = address.Street,
            City = address.City,
            Province = address.Province,
            ZipCode = address.ZipCode
        };
    }

    private static CmdDto.OrderItemResponse ToCommandDto(this OrderItem item)
    {
        return new CmdDto.OrderItemResponse
        {
            ProductId = item.ProductId,
            ProductName = item.ProductName,
            UnitPrice = item.UnitPrice.Amount,
            Currency = item.UnitPrice.Currency,
            Quantity = item.Quantity,
            LineTotal = item.LineTotal.Amount
        };
    }

    private static QryDto.AddressDto ToQueryDto(this Address address)
    {
        return new QryDto.AddressDto
        {
            Street = address.Street,
            City = address.City,
            Province = address.Province,
            ZipCode = address.ZipCode
        };
    }

    private static QryDto.OrderItemResponse ToQueryDto(this OrderItem item)
    {
        return new QryDto.OrderItemResponse
        {
            ProductId = item.ProductId,
            ProductName = item.ProductName,
            UnitPrice = item.UnitPrice.Amount,
            Currency = item.UnitPrice.Currency,
            Quantity = item.Quantity,
            LineTotal = item.LineTotal.Amount
        };
    }
}
