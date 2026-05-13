using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Catalog.Application.Abstractions;
using Catalog.Application.Products.Commands;
using Catalog.Domain.Entities;
using Shared.Contracts;
using Shared.Contracts.IntegrationEvents;
using Shared.Messaging.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Catalog.UnitTests.Application.Products.Commands;

public class CreateProductHandlerTests
{
    private Mock<ICatalogDbContext> _dbMock;
    private Mock<IEventBus> _eventBusMock;
    private Mock<ILogger<CreateProductHandler>> _loggerMock;
    private CreateProductHandler _handler;

    [SetUp]
    public void SetUp()
    {
        _dbMock = new Mock<ICatalogDbContext>();
        _eventBusMock = new Mock<IEventBus>();
        _loggerMock = new Mock<ILogger<CreateProductHandler>>();
        _handler = new CreateProductHandler(_dbMock.Object, _eventBusMock.Object, _loggerMock.Object);

        var productsMock = new Mock<DbSet<Product>>();
        _dbMock.Setup(x => x.Products).Returns(productsMock.Object);
    }

    [Test]
    public async Task Handle_ShouldReturnSuccess_WhenDataIsValid()
    {
        // Arrange
        var request = new CreateProductRequest
        {
            Name = "Product 1",
            Sku = "SKU123",
            Price = 100,
            Currency = "USD",
            InitialStockQuantity = 10
        };
        var command = new CreateProductCommand(request);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _dbMock.Verify(x => x.Products.Add(It.IsAny<Product>()), Times.Once);
        _eventBusMock.Verify(x => x.PublishAsync(It.IsAny<ProductCreatedIntegrationEvent>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        _dbMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
