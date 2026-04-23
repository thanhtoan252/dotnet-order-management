using Moq;
using FluentAssertions;
using NUnit.Framework;
using Microsoft.Extensions.Logging;
using Inventory.Application.EventHandlers;
using Inventory.Domain.Entities;
using Inventory.Domain.Repositories;
using Shared.Core.CQRS;
using Shared.Contracts.IntegrationEvents;

namespace Inventory.UnitTests.Application.EventHandlers;

[TestFixture]
public class ProductCreatedConsumerTests
{
    private Mock<IInventoryRepository> _inventoryRepoMock;
    private Mock<IUnitOfWork> _uowMock;
    private Mock<ILogger<ProductCreatedConsumer>> _loggerMock;
    private ProductCreatedConsumer _consumer;

    [SetUp]
    public void SetUp()
    {
        _inventoryRepoMock = new Mock<IInventoryRepository>();
        _uowMock = new Mock<IUnitOfWork>();
        _loggerMock = new Mock<ILogger<ProductCreatedConsumer>>();
        _consumer = new ProductCreatedConsumer(_inventoryRepoMock.Object, _uowMock.Object, _loggerMock.Object);
    }

    [Test]
    public async Task HandleAsync_Should_CreateInventoryItem_WhenNotExists()
    {
        // Arrange
        var productId = Guid.NewGuid();
        _inventoryRepoMock.Setup(x => x.ExistsForProductAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var @event = new ProductCreatedIntegrationEvent(Guid.NewGuid(), DateTime.UtcNow, productId, "SKU", "Name", 10);

        // Act
        await _consumer.HandleAsync(@event, CancellationToken.None);

        // Assert
        _inventoryRepoMock.Verify(x => x.Add(It.IsAny<InventoryItem>()), Times.Once);
        _uowMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
