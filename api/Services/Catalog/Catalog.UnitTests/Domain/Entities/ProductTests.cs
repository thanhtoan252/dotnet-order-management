using FluentAssertions;
using NUnit.Framework;
using Catalog.Domain.Entities;
using Shared.Core.ValueObjects;

namespace Catalog.UnitTests.Domain.Entities;

[TestFixture]
public class ProductTests
{
    [Test]
    public void Create_Should_ReturnSuccess_WhenDataIsValid()
    {
        // Arrange
        var name = "Test Product";
        var sku = "TEST-SKU";
        var price = Money.Create(100, "USD").Value;

        // Act
        var result = Product.Create(name, sku, price);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be(name);
        result.Value.SKU.Should().Be(sku);
        result.Value.Price.Should().Be(price);
    }

    [Test]
    public void UpdatePrice_Should_UpdatePrice()
    {
        // Arrange
        var product = Product.Create("P", "S", Money.Create(10).Value).Value;
        var newPrice = Money.Create(20).Value;

        // Act
        product.UpdatePrice(newPrice);

        // Assert
        product.Price.Should().Be(newPrice);
    }
}
