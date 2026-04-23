using FluentAssertions;
using NUnit.Framework;
using Catalog.Application.Products.Mappers;
using Catalog.Domain.Entities;
using Shared.Core.ValueObjects;

namespace Catalog.UnitTests.Application.Products.Mappers;

[TestFixture]
public class ProductMapperTests
{
    [Test]
    public void ToCommandResponse_Should_MapCorrectly()
    {
        // Arrange
        var product = Product.Create("Product", "SKU", Money.Create(10, "USD").Value, "Description").Value;

        // Act
        var response = product.ToCommandResponse();

        // Assert
        response.Id.Should().Be(product.Id);
        response.Name.Should().Be("Product");
        response.Sku.Should().Be("SKU");
        response.Price.Should().Be(10);
        response.Currency.Should().Be("USD");
        response.Description.Should().Be("Description");
    }

    [Test]
    public void ToQueryResponse_Should_MapCorrectly()
    {
        // Arrange
        var product = Product.Create("Product", "SKU", Money.Create(10, "USD").Value, "Description").Value;

        // Act
        var response = product.ToQueryResponse();

        // Assert
        response.Id.Should().Be(product.Id);
        response.Name.Should().Be("Product");
    }
}
