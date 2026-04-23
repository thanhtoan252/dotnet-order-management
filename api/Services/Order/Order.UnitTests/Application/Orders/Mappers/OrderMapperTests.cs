using FluentAssertions;
using NUnit.Framework;
using Order.Application.Orders.Mappers;
using Order.Domain.Entities;
using Shared.Core.ValueObjects;

namespace Order.UnitTests.Application.Orders.Mappers;

public class OrderMapperTests
{
    [Test]
    public void ToCommandResponse_ShouldMapCorrectly()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var address = Address.Create("Street", "City", "Province", "Zip");
        var order = OrderAggregate.Create(customerId, address).Value;
        order.AddItem(Guid.NewGuid(), "Product", Money.Create(10, "USD").Value, 2);

        // Act
        var response = order.ToCommandResponse();

        // Assert
        response.Id.Should().Be(order.Id);
        response.OrderNumber.Should().Be(order.OrderNumber);
        response.CustomerId.Should().Be(customerId);
        response.TotalAmount.Should().Be(20);
        response.Currency.Should().Be("USD");
        response.Items.Should().HaveCount(1);
        response.ShippingAddress.Street.Should().Be("Street");
    }
}
