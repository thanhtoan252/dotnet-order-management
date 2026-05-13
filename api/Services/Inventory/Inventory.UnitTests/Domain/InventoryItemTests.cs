using FluentAssertions;
using NUnit.Framework;
using Inventory.Domain.Entities;

namespace Inventory.UnitTests.Domain;

public class InventoryItemTests
{
    [Test]
    public void Create_ShouldReturnInventoryItem_WhenDataIsValid()
    {
        // Act
        var result = InventoryItem.Create(Guid.NewGuid(), "SKU123", "Product 1", 100);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.OnHand.Should().Be(100);
        result.Value.Reserved.Should().Be(0);
        result.Value.Available.Should().Be(100);
    }

    [Test]
    public void Reserve_ShouldUpdateReservedQuantity_WhenStockIsAvailable()
    {
        // Arrange
        var item = InventoryItem.Create(Guid.NewGuid(), "S", "P", 100).Value;

        // Act
        var result = item.Reserve(30);

        // Assert
        result.IsSuccess.Should().BeTrue();
        item.Reserved.Should().Be(30);
        item.Available.Should().Be(70);
    }

    [Test]
    public void Reserve_ShouldReturnFailure_WhenStockIsInsufficient()
    {
        // Arrange
        var item = InventoryItem.Create(Guid.NewGuid(), "S", "P", 10).Value;

        // Act
        var result = item.Reserve(20);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("InventoryItem.InsufficientStock");
    }

    [Test]
    public void Commit_ShouldDecreaseOnHandAndReserved_WhenStockWasReserved()
    {
        // Arrange
        var item = InventoryItem.Create(Guid.NewGuid(), "S", "P", 100).Value;
        item.Reserve(30);

        // Act
        var result = item.Commit(30);

        // Assert
        result.IsSuccess.Should().BeTrue();
        item.OnHand.Should().Be(70);
        item.Reserved.Should().Be(0);
    }
}
