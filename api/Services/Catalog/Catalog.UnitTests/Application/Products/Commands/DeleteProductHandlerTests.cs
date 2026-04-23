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
using Shared.Core.ValueObjects;
using Shared.Contracts.IntegrationEvents;

namespace Catalog.UnitTests.Application.Products.Commands;

[TestFixture]
public class DeleteProductHandlerTests
{
    private Mock<IProductRepository> _productRepoMock;
    private Mock<IUnitOfWork> _uowMock;
    private Mock<IEventBus> _eventBusMock;
    private Mock<ILogger<DeleteProductHandler>> _loggerMock;
    private DeleteProductHandler _handler;

    [SetUp]
    public void SetUp()
    {
        _productRepoMock = new Mock<IProductRepository>();
        _uowMock = new Mock<IUnitOfWork>();
        _eventBusMock = new Mock<IEventBus>();
        _loggerMock = new Mock<ILogger<DeleteProductHandler>>();

        _handler = new DeleteProductHandler(
            _productRepoMock.Object,
            _uowMock.Object,
            _eventBusMock.Object,
            _loggerMock.Object);
    }

    [Test]
    public async Task HandleAsync_Should_DeleteProduct_WhenFound()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var product = Product.Create("P", "S", Money.Create(10).Value).Value;

        _productRepoMock.Setup(x => x.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        // Act
        var result = await _handler.HandleAsync(new DeleteProductCommand(productId), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        product.IsDeleted.Should().BeTrue();
        _uowMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _eventBusMock.Verify(x => x.PublishAsync(It.IsAny<ProductDeletedIntegrationEvent>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
