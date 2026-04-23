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
public class OrderCancelledConsumerTests
{
    private Mock<IInventoryRepository> _inventoryRepoMock;
    private Mock<IUnitOfWork> _uowMock;
    private Mock<ILogger<OrderCancelledConsumer>> _loggerMock;
    private OrderCancelledConsumer _consumer;

    [SetUp]
    public void SetUp()
    {
        _inventoryRepoMock = new Mock<IInventoryRepository>();
        _uowMock = new Mock<IUnitOfWork>();
        _loggerMock = new Mock<ILogger<OrderCancelledConsumer>>();
        _consumer = new OrderCancelledConsumer(_inventoryRepoMock.Object, _uowMock.Object, _loggerMock.Object);
    }

    [Test]
    public async Task HandleAsync_Should_ReleaseStock_WhenOrderCancelled()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var item = InventoryItem.Create(productId, "S", "N", 10).Value;
        item.Reserve(5);
        _inventoryRepoMock.Setup(x => x.GetByProductIdsAsync(It.IsAny<List<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<InventoryItem> { item });

        var @event = new OrderCancelledIntegrationEvent(Guid.NewGuid(), DateTime.UtcNow, Guid.NewGuid(), "ORD-1", "Reason",
            new List<OrderLineItem> { new(productId, 5) });

        // Act
        await _consumer.HandleAsync(@event, CancellationToken.None);

        // Assert
        item.Reserved.Should().Be(0);
        _uowMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
