using Moq;
using FluentAssertions;
using NUnit.Framework;
using Microsoft.Extensions.Logging;
using Order.Application.Orders.Commands;
using Order.Domain.Entities;
using Order.Domain.Repositories;
using Shared.Core.CQRS;
using Shared.Core.Domain;
using Shared.Core.ValueObjects;
using Shared.Messaging.Abstractions;
using Shared.Contracts.IntegrationEvents;

namespace Order.UnitTests.Application.Orders.Commands;

[TestFixture]
public class CancelOrderHandlerTests
{
    private Mock<IOrderRepository> _orderRepoMock;
    private Mock<IUnitOfWork> _uowMock;
    private Mock<IEventBus> _eventBusMock;
    private Mock<ILogger<CancelOrderHandler>> _loggerMock;
    private CancelOrderHandler _handler;

    [SetUp]
    public void SetUp()
    {
        _orderRepoMock = new Mock<IOrderRepository>();
        _uowMock = new Mock<IUnitOfWork>();
        _eventBusMock = new Mock<IEventBus>();
        _loggerMock = new Mock<ILogger<CancelOrderHandler>>();
        _handler = new CancelOrderHandler(_orderRepoMock.Object, _uowMock.Object, _eventBusMock.Object, _loggerMock.Object);
    }

    [Test]
    public async Task HandleAsync_Should_CancelOrder_WhenOrderIsFound()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var address = Address.Create("S", "C", "P", "1");
        var order = OrderAggregate.Create(Guid.NewGuid(), address).Value;

        _orderRepoMock.Setup(x => x.GetByIdWithItemsAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        // Act
        var result = await _handler.HandleAsync(new CancelOrderCommand(orderId, "Reason", "user"), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        order.Status.Should().Be(Order.Domain.Entities.OrderStatus.Cancelled);
        _uowMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _eventBusMock.Verify(x => x.PublishAsync(It.IsAny<OrderCancelledIntegrationEvent>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
