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

namespace Order.UnitTests.Application.Orders.Commands;

[TestFixture]
public class ShipOrderHandlerTests
{
    private Mock<IOrderRepository> _orderRepoMock;
    private Mock<IUnitOfWork> _uowMock;
    private Mock<ILogger<ShipOrderHandler>> _loggerMock;
    private ShipOrderHandler _handler;

    [SetUp]
    public void SetUp()
    {
        _orderRepoMock = new Mock<IOrderRepository>();
        _uowMock = new Mock<IUnitOfWork>();
        _loggerMock = new Mock<ILogger<ShipOrderHandler>>();
        _handler = new ShipOrderHandler(_orderRepoMock.Object, _uowMock.Object, _loggerMock.Object);
    }

    [Test]
    public async Task HandleAsync_Should_ShipOrder_WhenOrderIsConfirmed()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var address = Address.Create("S", "C", "P", "1");
        var order = OrderAggregate.Create(Guid.NewGuid(), address).Value;
        order.AddItem(Guid.NewGuid(), "P", Money.Create(10).Value, 1);
        order.Confirm("user");

        _orderRepoMock.Setup(x => x.GetByIdWithItemsAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        // Act
        var result = await _handler.HandleAsync(new ShipOrderCommand(orderId, "user"), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        order.Status.Should().Be(Order.Domain.Entities.OrderStatus.Shipped);
        _uowMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
