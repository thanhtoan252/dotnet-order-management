using Order.Application.Orders.Models;
using Order.Domain.Entities;
using CmdDto = Order.Api.Endpoints.Orders.V1.Commands.DTOs;
using QryDto = Order.Api.Endpoints.Orders.V1.Queries.DTOs;

namespace Order.Api.Endpoints.Orders.V1.Mappers;

internal static class OrderApiMapper
{
    internal static PlaceOrderInput ToInput(this CmdDto.PlaceOrderRequest request)
    {
        return new PlaceOrderInput(
            request.CustomerId,
            request.ShippingAddress.ToInput(),
            request.Lines.Select(l => l.ToInput()).ToList(),
            request.Notes);
    }

    internal static CmdDto.OrderResponse ToCommandDto(this OrderResult order)
    {
        return new CmdDto.OrderResponse
        {
            Id = order.Id,
            OrderNumber = order.OrderNumber,
            CustomerId = order.CustomerId,
            Status = order.Status.ToCommandDto(),
            TotalAmount = order.TotalAmount,
            Currency = order.Currency,
            ShippingAddress = order.ShippingAddress.ToCommandDto(),
            CreatedAt = order.CreatedAt,
            Items = order.Items.Select(i => i.ToCommandDto()).ToList()
        };
    }

    internal static QryDto.OrderResponse ToQueryDto(this OrderResult order)
    {
        return new QryDto.OrderResponse
        {
            Id = order.Id,
            OrderNumber = order.OrderNumber,
            CustomerId = order.CustomerId,
            Status = order.Status.ToQueryDto(),
            TotalAmount = order.TotalAmount,
            Currency = order.Currency,
            ShippingAddress = order.ShippingAddress.ToQueryDto(),
            CreatedAt = order.CreatedAt,
            Items = order.Items.Select(i => i.ToQueryDto()).ToList()
        };
    }

    private static AddressInput ToInput(this CmdDto.AddressDto address)
    {
        return new AddressInput(address.Street, address.City, address.Province, address.ZipCode);
    }

    private static OrderLineInput ToInput(this CmdDto.OrderLineRequest line)
    {
        return new OrderLineInput(
            line.ProductId,
            line.ProductName,
            line.UnitPrice,
            line.Currency,
            line.Quantity);
    }

    private static CmdDto.AddressDto ToCommandDto(this AddressResult address)
    {
        return new CmdDto.AddressDto
        {
            Street = address.Street,
            City = address.City,
            Province = address.Province,
            ZipCode = address.ZipCode
        };
    }

    private static QryDto.AddressDto ToQueryDto(this AddressResult address)
    {
        return new QryDto.AddressDto
        {
            Street = address.Street,
            City = address.City,
            Province = address.Province,
            ZipCode = address.ZipCode
        };
    }

    private static CmdDto.OrderItemResponse ToCommandDto(this OrderItemResult item)
    {
        return new CmdDto.OrderItemResponse
        {
            ProductId = item.ProductId,
            ProductName = item.ProductName,
            UnitPrice = item.UnitPrice,
            Currency = item.Currency,
            Quantity = item.Quantity,
            LineTotal = item.LineTotal
        };
    }

    private static QryDto.OrderItemResponse ToQueryDto(this OrderItemResult item)
    {
        return new QryDto.OrderItemResponse
        {
            ProductId = item.ProductId,
            ProductName = item.ProductName,
            UnitPrice = item.UnitPrice,
            Currency = item.Currency,
            Quantity = item.Quantity,
            LineTotal = item.LineTotal
        };
    }

    private static CmdDto.OrderStatus ToCommandDto(this OrderStatus status)
    {
        return Enum.Parse<CmdDto.OrderStatus>(status.ToString());
    }

    private static QryDto.OrderStatus ToQueryDto(this OrderStatus status)
    {
        return Enum.Parse<QryDto.OrderStatus>(status.ToString());
    }
}
