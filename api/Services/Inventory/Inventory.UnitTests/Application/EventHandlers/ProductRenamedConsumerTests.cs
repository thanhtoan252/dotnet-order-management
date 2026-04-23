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
public class ProductRenamedConsumerTests
{
    private Mock<IInventoryRepository> _inventoryRepoMock;
    private Mock<IUnitOfWork> _uowMock;
    private Mock<ILogger<ProductRenamedConsumer>> _loggerMock;
    private ProductRenamedConsumer _consumer;

    [SetUp]
    public void SetUp()
    {
        _inventoryRepoMock = new Mock<IInventoryRepository>();
        _uowMock = new Mock<IUnitOfWork>();
        _loggerMock = new Mock<ILogger<ProductRenamedConsumer>>();
        _consumer = new ProductRenamedConsumer(_inventoryRepoMock.Object, _uowMock.Object, _loggerMock.Object);
    }

    [Test]
    public async Task HandleAsync_Should_RenameProduct_WhenExists()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var item = InventoryItem.Create(productId, "S", "Old Name", 10).Value;
        _inventoryRepoMock.Setup(x => x.GetByProductIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(item);

        var @event = new ProductRenamedIntegrationEvent(Guid.NewGuid(), DateTime.UtcNow, productId, "New Name");

        // Act
        await _consumer.HandleAsync(@event, CancellationToken.None);

        // Assert
        item.ProductName.Should().Be("New Name");
        _uowMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
