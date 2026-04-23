using FluentAssertions;
using NUnit.Framework;
using Order.Application.Orders.Mappers;
using Order.Domain.Entities;
using Shared.Core.ValueObjects;

namespace Order.UnitTests.Application.Orders.Mappers;

[TestFixture]
public class OrderMapperTests
{
    [Test]
    public void ToCommandResponse_Should_MapCorrectly()
    {
        // Arrange
        var address = Address.Create("Street", "City", "Province", "12345");
        var order = OrderAggregate.Create(Guid.NewGuid(), address).Value;
        order.AddItem(Guid.NewGuid(), "Product", Money.Create(10).Value, 2);

        // Act
        var response = order.ToCommandResponse();

        // Assert
        response.OrderNumber.Should().Be(order.OrderNumber);
        response.TotalAmount.Should().Be(20);
        response.Items.Should().HaveCount(1);
        response.ShippingAddress.Street.Should().Be("Street");
    }
}
