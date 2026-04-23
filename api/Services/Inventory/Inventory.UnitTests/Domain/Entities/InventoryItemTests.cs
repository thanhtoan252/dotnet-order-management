using FluentAssertions;
using NUnit.Framework;
using Inventory.Domain.Entities;

namespace Inventory.UnitTests.Domain.Entities;

[TestFixture]
public class InventoryItemTests
{
    [Test]
    public void Create_Should_ReturnSuccess_WhenDataIsValid()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var sku = "TEST-SKU";
        var name = "Test Product";
        var quantity = 100;

        // Act
        var result = InventoryItem.Create(productId, sku, name, quantity);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.ProductId.Should().Be(productId);
        result.Value.OnHand.Should().Be(quantity);
        result.Value.Reserved.Should().Be(0);
        result.Value.Available.Should().Be(quantity);
    }

    [Test]
    public void Reserve_Should_IncreaseReserved_WhenStockIsAvailable()
    {
        // Arrange
        var item = InventoryItem.Create(Guid.NewGuid(), "S", "N", 10).Value;

        // Act
        var result = item.Reserve(5);

        // Assert
        result.IsSuccess.Should().BeTrue();
        item.Reserved.Should().Be(5);
        item.Available.Should().Be(5);
    }

    [Test]
    public void Reserve_Should_Fail_WhenStockIsNotAvailable()
    {
        // Arrange
        var item = InventoryItem.Create(Guid.NewGuid(), "S", "N", 10).Value;

        // Act
        var result = item.Reserve(15);

        // Assert
        result.IsFailure.Should().BeTrue();
    }

    [Test]
    public void Receive_Should_IncreaseOnHand()
    {
        // Arrange
        var item = InventoryItem.Create(Guid.NewGuid(), "S", "N", 10).Value;

        // Act
        item.Receive(5);

        // Assert
        item.OnHand.Should().Be(15);
    }

    [Test]
    public void Commit_Should_DecreaseOnHandAndReserved()
    {
        // Arrange
        var item = InventoryItem.Create(Guid.NewGuid(), "S", "N", 10).Value;
        item.Reserve(5);

        // Act
        item.Commit(5);

        // Assert
        item.OnHand.Should().Be(5);
        item.Reserved.Should().Be(0);
    }

    [Test]
    public void Release_Should_DecreaseReserved()
    {
        // Arrange
        var item = InventoryItem.Create(Guid.NewGuid(), "S", "N", 10).Value;
        item.Reserve(5);

        // Act
        item.Release(5);

        // Assert
        item.OnHand.Should().Be(10);
        item.Reserved.Should().Be(0);
    }

    [Test]
    public void SetOnHand_Should_UpdateOnHand_WhenValueIsAtLeastReserved()
    {
        // Arrange
        var item = InventoryItem.Create(Guid.NewGuid(), "S", "N", 10).Value;
        item.Reserve(5);

        // Act
        var result = item.SetOnHand(7);

        // Assert
        result.IsSuccess.Should().BeTrue();
        item.OnHand.Should().Be(7);
    }

    [Test]
    public void SetOnHand_Should_Fail_WhenValueIsLessThanReserved()
    {
        // Arrange
        var item = InventoryItem.Create(Guid.NewGuid(), "S", "N", 10).Value;
        item.Reserve(5);

        // Act
        var result = item.SetOnHand(3);

        // Assert
        result.IsFailure.Should().BeTrue();
    }
}
