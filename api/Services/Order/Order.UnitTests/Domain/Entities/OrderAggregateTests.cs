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
}
