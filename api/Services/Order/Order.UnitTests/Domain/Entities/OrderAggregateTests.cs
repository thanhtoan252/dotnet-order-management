using FluentAssertions;
using NUnit.Framework;
using Order.Domain;
using Order.Domain.Entities;
using Shared.Core.ValueObjects;

namespace Order.UnitTests.Domain.Entities;

[TestFixture]
public class OrderAggregateTests
{
    private Address _validAddress;

    [SetUp]
    public void SetUp()
    {
        _validAddress = Address.Create("Street", "City", "Province", "12345");
    }

    [Test]
    public void Create_Should_ReturnSuccess_WhenDataIsValid()
    {
        // Arrange
        var customerId = Guid.NewGuid();

        // Act
        var result = OrderAggregate.Create(customerId, _validAddress);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.CustomerId.Should().Be(customerId);
        result.Value.Status.Should().Be(OrderStatus.Pending);
    }

    [Test]
    public void Confirm_Should_Fail_WhenOrderHasNoItems()
    {
        // Arrange
        var order = OrderAggregate.Create(Guid.NewGuid(), _validAddress).Value;

        // Act
        var result = order.Confirm("test-user");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(DomainErrors.Order.EmptyItems);
    }

    [Test]
    public void Confirm_Should_Succeed_WhenOrderHasItems()
    {
        // Arrange
        var order = OrderAggregate.Create(Guid.NewGuid(), _validAddress).Value;
        order.AddItem(Guid.NewGuid(), "Product 1", Money.Create(10, "USD").Value, 1);

        // Act
        var result = order.Confirm("test-user");

        // Assert
        result.IsSuccess.Should().BeTrue();
        order.Status.Should().Be(OrderStatus.Confirmed);
        order.ConfirmedAt.Should().NotBeNull();
    }

    [Test]
    public void AddItem_Should_Fail_WhenCurrencyIsDifferent()
    {
        // Arrange
        var order = OrderAggregate.Create(Guid.NewGuid(), _validAddress).Value;
        order.AddItem(Guid.NewGuid(), "P1", Money.Create(10, "USD").Value, 1);

        // Act
        var result = order.AddItem(Guid.NewGuid(), "P2", Money.Create(10, "EUR").Value, 1);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(DomainErrors.Order.CurrencyMismatch);
    }

    [Test]
    public void Ship_Should_Succeed_WhenOrderIsConfirmed()
    {
        // Arrange
        var order = OrderAggregate.Create(Guid.NewGuid(), _validAddress).Value;
        order.AddItem(Guid.NewGuid(), "P1", Money.Create(10).Value, 1);
        order.Confirm("user");

        // Act
        var result = order.Ship("user");

        // Assert
        result.IsSuccess.Should().BeTrue();
        order.Status.Should().Be(OrderStatus.Shipped);
        order.ShippedAt.Should().NotBeNull();
    }

    [Test]
    public void Ship_Should_Fail_WhenOrderIsPending()
    {
        // Arrange
        var order = OrderAggregate.Create(Guid.NewGuid(), _validAddress).Value;

        // Act
        var result = order.Ship("user");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Order.InvalidState");
    }

    [Test]
    public void MarkDelivered_Should_Succeed_WhenOrderIsShipped()
    {
        // Arrange
        var order = OrderAggregate.Create(Guid.NewGuid(), _validAddress).Value;
        order.AddItem(Guid.NewGuid(), "P1", Money.Create(10).Value, 1);
        order.Confirm("user");
        order.Ship("user");

        // Act
        var result = order.MarkDelivered();

        // Assert
        result.IsSuccess.Should().BeTrue();
        order.Status.Should().Be(OrderStatus.Delivered);
        order.DeliveredAt.Should().NotBeNull();
    }

    [Test]
    public void Cancel_Should_Succeed_WhenOrderIsPending()
    {
        // Arrange
        var order = OrderAggregate.Create(Guid.NewGuid(), _validAddress).Value;

        // Act
        var result = order.Cancel("reason", "user");

        // Assert
        result.IsSuccess.Should().BeTrue();
        order.Status.Should().Be(OrderStatus.Cancelled);
        order.CancelledAt.Should().NotBeNull();
    }

    [Test]
    public void Cancel_Should_Fail_WhenOrderIsShipped()
    {
        // Arrange
        var order = OrderAggregate.Create(Guid.NewGuid(), _validAddress).Value;
        order.AddItem(Guid.NewGuid(), "P1", Money.Create(10).Value, 1);
        order.Confirm("user");
        order.Ship("user");

        // Act
        var result = order.Cancel("reason", "user");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Order.InvalidState");
    }
}
