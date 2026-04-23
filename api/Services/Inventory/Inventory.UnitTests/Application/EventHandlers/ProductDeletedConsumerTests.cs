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
public class ProductDeletedConsumerTests
{
    private Mock<IInventoryRepository> _inventoryRepoMock;
    private Mock<IUnitOfWork> _uowMock;
    private Mock<ILogger<ProductDeletedConsumer>> _loggerMock;
    private ProductDeletedConsumer _consumer;

    [SetUp]
    public void SetUp()
    {
        _inventoryRepoMock = new Mock<IInventoryRepository>();
        _uowMock = new Mock<IUnitOfWork>();
        _loggerMock = new Mock<ILogger<ProductDeletedConsumer>>();
        _consumer = new ProductDeletedConsumer(_inventoryRepoMock.Object, _uowMock.Object, _loggerMock.Object);
    }

    [Test]
    public async Task HandleAsync_Should_SoftDeleteProduct_WhenExists()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var item = InventoryItem.Create(productId, "S", "Name", 10).Value;
        _inventoryRepoMock.Setup(x => x.GetByProductIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(item);

        var @event = new ProductDeletedIntegrationEvent(Guid.NewGuid(), DateTime.UtcNow, productId);

        // Act
        await _consumer.HandleAsync(@event, CancellationToken.None);

        // Assert
        item.IsDeleted.Should().BeTrue();
        _uowMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
