using Moq;
using FluentAssertions;
using NUnit.Framework;
using Catalog.Application.Products.Queries;
using Catalog.Domain.Entities;
using Catalog.Domain.Repositories;
using Shared.Core.CQRS;
using Shared.Core.ValueObjects;

namespace Catalog.UnitTests.Application.Products.Queries;

[TestFixture]
public class GetAllProductsHandlerTests
{
    private Mock<IProductRepository> _productRepoMock;
    private GetAllProductsHandler _handler;

    [SetUp]
    public void SetUp()
    {
        _productRepoMock = new Mock<IProductRepository>();
        _handler = new GetAllProductsHandler(_productRepoMock.Object);
    }

    [Test]
    public async Task HandleAsync_Should_ReturnProducts()
    {
        // Arrange
        var products = new List<Product>
        {
            Product.Create("P1", "S1", Money.Create(10).Value).Value,
            Product.Create("P2", "S2", Money.Create(20).Value).Value
        };

        _productRepoMock.Setup(x => x.GetAllAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(products);

        // Act
        var result = await _handler.HandleAsync(new GetAllProductsQuery(), CancellationToken.None);

        // Assert
        result.Should().HaveCount(2);
        result[0].Name.Should().Be("P1");
    }
}
