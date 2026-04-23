using FluentAssertions;
using NUnit.Framework;
using Order.Domain.Entities;
using Shared.Core.ValueObjects;

namespace Order.UnitTests.Domain.Entities;

[TestFixture]
public class OrderItemTests
{
    [Test]
    public void LineTotal_Should_BeZero_WhenCancelled()
    {
        // Arrange
        var item = OrderItem.Create(Guid.NewGuid(), Guid.NewGuid(), "P", Money.Create(10).Value, 2);

        // Act
        item.MarkCancelled();

        // Assert
        item.LineTotal.Amount.Should().Be(0);
    }

    [Test]
    public void LineTotal_Should_BeCorrect_WhenNotCancelled()
    {
        // Arrange
        var item = OrderItem.Create(Guid.NewGuid(), Guid.NewGuid(), "P", Money.Create(10).Value, 2);

        // Assert
        item.LineTotal.Amount.Should().Be(20);
    }
}
