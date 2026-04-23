using Moq;
using FluentAssertions;
using NUnit.Framework;
using Microsoft.Extensions.Logging;
using Inventory.Application.EventHandlers;
using Inventory.Domain.Entities;
using Inventory.Domain.Repositories;
using Shared.Core.CQRS;
using Shared.Messaging.Abstractions;
using Shared.Contracts.IntegrationEvents;

namespace Inventory.UnitTests.Application.EventHandlers;

[TestFixture]
public class OrderPlacedConsumerTests
{
    private Mock<IInventoryRepository> _inventoryRepoMock;
    private Mock<IUnitOfWork> _uowMock;
    private Mock<IEventBus> _eventBusMock;
    private Mock<ILogger<OrderPlacedConsumer>> _loggerMock;
    private OrderPlacedConsumer _consumer;

    [SetUp]
    public void SetUp()
    {
        _inventoryRepoMock = new Mock<IInventoryRepository>();
        _uowMock = new Mock<IUnitOfWork>();
        _eventBusMock = new Mock<IEventBus>();
        _loggerMock = new Mock<ILogger<OrderPlacedConsumer>>();
        _consumer = new OrderPlacedConsumer(_inventoryRepoMock.Object, _uowMock.Object, _eventBusMock.Object, _loggerMock.Object);
    }

    [Test]
    public async Task HandleAsync_Should_ReserveStock_WhenStockIsAvailable()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var item = InventoryItem.Create(productId, "S", "N", 10).Value;
        _inventoryRepoMock.Setup(x => x.GetByProductIdsAsync(It.IsAny<List<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<InventoryItem> { item });

        var @event = new OrderPlacedIntegrationEvent(Guid.NewGuid(), DateTime.UtcNow, Guid.NewGuid(), "ORD-1", Guid.NewGuid(),
            new List<OrderLineItem> { new(productId, 5) });

        // Act
        await _consumer.HandleAsync(@event, CancellationToken.None);

        // Assert
        item.Reserved.Should().Be(5);
        _uowMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _eventBusMock.Verify(x => x.PublishAsync(It.IsAny<StockReservedIntegrationEvent>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
