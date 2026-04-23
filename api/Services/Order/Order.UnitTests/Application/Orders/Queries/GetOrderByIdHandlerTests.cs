using Moq;
using FluentAssertions;
using NUnit.Framework;
using Order.Application.Orders.Queries;
using Order.Domain.Entities;
using Order.Domain.Repositories;
using Shared.Core.CQRS;
using Shared.Core.Domain;
using Shared.Core.ValueObjects;

namespace Order.UnitTests.Application.Orders.Queries;

[TestFixture]
public class GetOrderByIdHandlerTests
{
    private Mock<IOrderRepository> _orderRepoMock;
    private GetOrderByIdHandler _handler;

    [SetUp]
    public void SetUp()
    {
        _orderRepoMock = new Mock<IOrderRepository>();
        _handler = new GetOrderByIdHandler(_orderRepoMock.Object);
    }

    [Test]
    public async Task HandleAsync_Should_ReturnOrder_WhenFound()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var address = Address.Create("S", "C", "P", "1");
        var order = OrderAggregate.Create(Guid.NewGuid(), address).Value;

        _orderRepoMock.Setup(x => x.GetByIdWithItemsAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        // Act
        var result = await _handler.HandleAsync(new GetOrderByIdQuery(orderId), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(order.Id);
    }

    [Test]
    public async Task HandleAsync_Should_ReturnFailure_WhenNotFound()
    {
        // Arrange
        _orderRepoMock.Setup(x => x.GetByIdWithItemsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((OrderAggregate?)null);

        // Act
        var result = await _handler.HandleAsync(new GetOrderByIdQuery(Guid.NewGuid()), CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Order.NotFound");
    }
}
