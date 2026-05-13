using FluentAssertions;
using NUnit.Framework;
using Order.Domain.Entities;
using Shared.Core.ValueObjects;

namespace Order.UnitTests.Domain;

public class OrderAggregateTests
{
    private Address _address;
    private Guid _customerId;

    [SetUp]
    public void SetUp()
    {
        _address = Address.Create("123 Main St", "Seattle", "WA", "98101");
        _customerId = Guid.NewGuid();
    }

    [Test]
    public void Create_ShouldReturnOrder_WhenDataIsValid()
    {
        // Act
        var result = OrderAggregate.Create(_customerId, _address);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.CustomerId.Should().Be(_customerId);
        result.Value.Status.Should().Be(OrderStatus.Pending);
        result.Value.DomainEvents.Should().HaveCount(1);
    }

    [Test]
    public void AddItem_ShouldUpdateTotal_WhenItemIsAdded()
    {
        // Arrange
        var order = OrderAggregate.Create(_customerId, _address).Value;
        var unitPrice = Money.Create(100, "USD").Value;

        // Act
        var result = order.AddItem(Guid.NewGuid(), "Product 1", unitPrice, 2);

        // Assert
        result.IsSuccess.Should().BeTrue();
        order.Items.Should().HaveCount(1);
        order.TotalAmount.Amount.Should().Be(200);
    }

    [Test]
    public void AddItem_ShouldReturnFailure_WhenStatusIsNotPending()
    {
        // Arrange
        var order = OrderAggregate.Create(_customerId, _address).Value;
        order.AddItem(Guid.NewGuid(), "Product 1", Money.Create(10, "USD").Value, 1);
        order.Confirm("tester");

        // Act
        var result = order.AddItem(Guid.NewGuid(), "Product 2", Money.Create(10, "USD").Value, 1);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Order.InvalidState");
    }

    [Test]
    public void Confirm_ShouldChangeStatus_WhenOrderIsPendingAndHasItems()
    {
        // Arrange
        var order = OrderAggregate.Create(_customerId, _address).Value;
        order.AddItem(Guid.NewGuid(), "Product 1", Money.Create(10, "USD").Value, 1);

        // Act
        var result = order.Confirm("tester");

        // Assert
        result.IsSuccess.Should().BeTrue();
        order.Status.Should().Be(OrderStatus.Confirmed);
        order.ConfirmedAt.Should().NotBeNull();
    }

    [Test]
    public void Confirm_ShouldReturnFailure_WhenOrderHasNoItems()
    {
        // Arrange
        var order = OrderAggregate.Create(_customerId, _address).Value;

        // Act
        var result = order.Confirm("tester");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Order.EmptyItems");
    }

    [Test]
    public void Cancel_ShouldChangeStatus_WhenAllowed()
    {
        // Arrange
        var order = OrderAggregate.Create(_customerId, _address).Value;

        // Act
        var result = order.Cancel("Changed mind", "tester");

        // Assert
        result.IsSuccess.Should().BeTrue();
        order.Status.Should().Be(OrderStatus.Cancelled);
        order.CancelledAt.Should().NotBeNull();
    }
}
