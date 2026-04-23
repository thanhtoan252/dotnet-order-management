using Moq;
using FluentAssertions;
using NUnit.Framework;
using Microsoft.Extensions.Logging;
using Order.Application.EventHandlers;
using Order.Domain.Entities;
using Order.Domain.Repositories;
using Shared.Core.CQRS;
using Shared.Contracts.IntegrationEvents;
using Shared.Core.ValueObjects;

namespace Order.UnitTests.Application.EventHandlers;

[TestFixture]
public class StockReservedConsumerTests
{
    private Mock<IOrderRepository> _orderRepoMock;
    private Mock<IUnitOfWork> _uowMock;
    private Mock<ILogger<StockReservedConsumer>> _loggerMock;
    private StockReservedConsumer _consumer;

    [SetUp]
    public void SetUp()
    {
        _orderRepoMock = new Mock<IOrderRepository>();
        _uowMock = new Mock<IUnitOfWork>();
        _loggerMock = new Mock<ILogger<StockReservedConsumer>>();
        _consumer = new StockReservedConsumer(_orderRepoMock.Object, _uowMock.Object, _loggerMock.Object);
    }

    [Test]
    public async Task HandleAsync_Should_ConfirmOrder_WhenStockIsReserved()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var address = Address.Create("S", "C", "P", "1");
        var order = OrderAggregate.Create(Guid.NewGuid(), address).Value;
        order.AddItem(Guid.NewGuid(), "P", Money.Create(10).Value, 1);

        _orderRepoMock.Setup(x => x.GetByIdWithItemsAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        var @event = new StockReservedIntegrationEvent(Guid.NewGuid(), DateTime.UtcNow, orderId, new List<ReservedItem>());

        // Act
        await _consumer.HandleAsync(@event, CancellationToken.None);

        // Assert
        order.Status.Should().Be(Order.Domain.Entities.OrderStatus.Confirmed);
        _uowMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
