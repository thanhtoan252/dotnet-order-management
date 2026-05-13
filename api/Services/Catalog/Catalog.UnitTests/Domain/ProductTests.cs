using FluentAssertions;
using NUnit.Framework;
using Catalog.Domain.Entities;
using Shared.Core.ValueObjects;

namespace Catalog.UnitTests.Domain;

public class ProductTests
{
    [Test]
    public void Create_ShouldReturnProduct_WhenDataIsValid()
    {
        // Arrange
        var price = Money.Create(100, "USD").Value;

        // Act
        var result = Product.Create("Product 1", "SKU123", price, "Description");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("Product 1");
        result.Value.SKU.Should().Be("SKU123");
        result.Value.Price.Should().Be(price);
    }

    [Test]
    public void Create_ShouldReturnFailure_WhenNameIsEmpty()
    {
        // Act
        var result = Product.Create("", "SKU123", Money.Create(10, "USD").Value);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Product.InvalidName");
    }

    [Test]
    public void UpdatePrice_ShouldUpdatePrice_WhenValidPriceProvided()
    {
        // Arrange
        var product = Product.Create("P", "S", Money.Create(10, "USD").Value).Value;
        var newPrice = Money.Create(20, "USD").Value;

        // Act
        product.UpdatePrice(newPrice);

        // Assert
        product.Price.Should().Be(newPrice);
    }
}
