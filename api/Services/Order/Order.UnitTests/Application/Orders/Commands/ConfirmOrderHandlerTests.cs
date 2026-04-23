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
public class ConfirmOrderHandlerTests
{
    private Mock<IOrderRepository> _orderRepoMock;
    private Mock<IUnitOfWork> _uowMock;
    private Mock<ILogger<ConfirmOrderHandler>> _loggerMock;
    private ConfirmOrderHandler _handler;

    [SetUp]
    public void SetUp()
    {
        _orderRepoMock = new Mock<IOrderRepository>();
        _uowMock = new Mock<IUnitOfWork>();
        _loggerMock = new Mock<ILogger<ConfirmOrderHandler>>();
        _handler = new ConfirmOrderHandler(_orderRepoMock.Object, _uowMock.Object, _loggerMock.Object);
    }

    [Test]
    public async Task HandleAsync_Should_ReturnSuccess_WhenOrderIsFoundAndPending()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var address = Address.Create("S", "C", "P", "1");
        var order = OrderAggregate.Create(Guid.NewGuid(), address).Value;
        order.AddItem(Guid.NewGuid(), "P", Money.Create(10).Value, 1);

        _orderRepoMock.Setup(x => x.GetByIdWithItemsAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        // Act
        var result = await _handler.HandleAsync(new ConfirmOrderCommand(orderId, "user"), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        order.Status.Should().Be(Order.Domain.Entities.OrderStatus.Confirmed);
        _uowMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task HandleAsync_Should_ReturnFailure_WhenOrderNotFound()
    {
        // Arrange
        _orderRepoMock.Setup(x => x.GetByIdWithItemsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((OrderAggregate)null);

        // Act
        var result = await _handler.HandleAsync(new ConfirmOrderCommand(Guid.NewGuid(), "user"), CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Order.NotFound");
    }
}
