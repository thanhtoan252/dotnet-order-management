using Moq;
using FluentAssertions;
using NUnit.Framework;
using Microsoft.Extensions.Logging;
using Catalog.Application.Products.Commands;
using Catalog.Domain.Entities;
using Catalog.Domain.Repositories;
using Shared.Core.CQRS;
using Shared.Messaging.Abstractions;
using Shared.Core.Domain;
using Shared.Contracts;
using Shared.Contracts.IntegrationEvents;

namespace Catalog.UnitTests.Application.Products.Commands;

[TestFixture]
public class CreateProductHandlerTests
{
    private Mock<IProductRepository> _productRepoMock;
    private Mock<IUnitOfWork> _uowMock;
    private Mock<IEventBus> _eventBusMock;
    private Mock<ILogger<CreateProductHandler>> _loggerMock;
    private CreateProductHandler _handler;

    [SetUp]
    public void SetUp()
    {
        _productRepoMock = new Mock<IProductRepository>();
        _uowMock = new Mock<IUnitOfWork>();
        _eventBusMock = new Mock<IEventBus>();
        _loggerMock = new Mock<ILogger<CreateProductHandler>>();

        _handler = new CreateProductHandler(
            _productRepoMock.Object,
            _uowMock.Object,
            _eventBusMock.Object,
            _loggerMock.Object);
    }

    [Test]
    public async Task HandleAsync_Should_ReturnSuccess_WhenDataIsValid()
    {
        // Arrange
        var request = new CreateProductRequest
        {
            Name = "Product",
            Sku = "SKU",
            Price = 10,
            Currency = "USD"
        };
        var command = new CreateProductCommand(request);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _productRepoMock.Verify(x => x.Add(It.IsAny<Product>()), Times.Once);
        _eventBusMock.Verify(x => x.PublishAsync(It.IsAny<ProductCreatedIntegrationEvent>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        _uowMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
