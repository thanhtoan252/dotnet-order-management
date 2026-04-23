using Moq;
using FluentAssertions;
using NUnit.Framework;
using Microsoft.Extensions.Logging;
using Order.Application.Orders.Commands;
using Order.Application.Services;
using Order.Domain.Repositories;
using Shared.Core.CQRS;
using Shared.Messaging.Abstractions;
using Shared.Core.Domain;
using Shared.Contracts;

namespace Order.UnitTests.Application.Orders.Commands;

[TestFixture]
public class PlaceOrderHandlerTests
{
    private Mock<IOrderRepository> _orderRepoMock;
    private Mock<IUnitOfWork> _uowMock;
    private Mock<IEventBus> _eventBusMock;
    private Mock<IInventoryService> _inventoryServiceMock;
    private Mock<ILogger<PlaceOrderHandler>> _loggerMock;
    private PlaceOrderHandler _handler;

    [SetUp]
    public void SetUp()
    {
        _orderRepoMock = new Mock<IOrderRepository>();
        _uowMock = new Mock<IUnitOfWork>();
        _eventBusMock = new Mock<IEventBus>();
        _inventoryServiceMock = new Mock<IInventoryService>();
        _loggerMock = new Mock<ILogger<PlaceOrderHandler>>();

        _handler = new PlaceOrderHandler(
            _orderRepoMock.Object,
            _uowMock.Object,
            _eventBusMock.Object,
            _inventoryServiceMock.Object,
            _loggerMock.Object);
    }

    [Test]
    public async Task HandleAsync_Should_ReturnFailure_WhenStockIsNotAvailable()
    {
        // Arrange
        var request = new PlaceOrderRequest
        {
            CustomerId = Guid.NewGuid(),
            ShippingAddress = new AddressDto { Street = "S", City = "C", Province = "P", ZipCode = "123" },
            Lines = new List<OrderLineRequest> { new() { ProductId = Guid.NewGuid(), Quantity = 1, UnitPrice = 10 } }
        };
        var command = new PlaceOrderCommand(request, "user");

        _inventoryServiceMock.Setup(x => x.CheckAvailabilityAsync(It.IsAny<StockCheckRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new StockCheckResponse(false, new List<StockCheckFailure> { new(request.Lines.First().ProductId, "No stock") }));

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Order.InsufficientStock");
    }
}
