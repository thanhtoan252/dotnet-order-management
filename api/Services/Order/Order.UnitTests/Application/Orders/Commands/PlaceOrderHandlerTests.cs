using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Order.Application.Abstractions;
using Order.Application.Orders.Commands;
using Order.Application.Services;
using Order.Domain.Entities;
using Shared.Contracts;
using Shared.Contracts.IntegrationEvents;
using Shared.Messaging.Abstractions;

namespace Order.UnitTests.Application.Orders.Commands;

public class PlaceOrderHandlerTests
{
    private Mock<IOrderDbContext> _dbMock;
    private Mock<IEventBus> _eventBusMock;
    private Mock<IInventoryService> _inventoryServiceMock;
    private Mock<ILogger<PlaceOrderHandler>> _loggerMock;
    private PlaceOrderHandler _handler;

    [SetUp]
    public void SetUp()
    {
        _dbMock = new Mock<IOrderDbContext>();
        _eventBusMock = new Mock<IEventBus>();
        _inventoryServiceMock = new Mock<IInventoryService>();
        _loggerMock = new Mock<ILogger<PlaceOrderHandler>>();

        _handler = new PlaceOrderHandler(
            _dbMock.Object,
            _eventBusMock.Object,
            _inventoryServiceMock.Object,
            _loggerMock.Object);

        var ordersMock = new Mock<Microsoft.EntityFrameworkCore.DbSet<OrderAggregate>>();
        _dbMock.Setup(x => x.Orders).Returns(ordersMock.Object);
    }

    [Test]
    public async Task Handle_ShouldReturnSuccess_WhenStockIsAvailableAndDataIsValid()
    {
        // Arrange
        var request = new PlaceOrderRequest
        {
            CustomerId = Guid.NewGuid(),
            ShippingAddress = new AddressDto
            {
                Street = "123 Main St",
                City = "Seattle",
                Province = "WA",
                ZipCode = "98101"
            },
            Lines = new List<OrderLineRequest>
            {
                new() { ProductId = Guid.NewGuid(), Quantity = 2, UnitPrice = 100, Currency = "USD" }
            }
        };

        var command = new PlaceOrderCommand(request, "tester");

        _inventoryServiceMock.Setup(x => x.CheckAvailabilityAsync(It.IsAny<StockCheckRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new StockCheckResponse(true, new List<StockCheckFailure>()));

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _dbMock.Verify(x => x.Orders.Add(It.IsAny<OrderAggregate>()), Times.Once);
        _eventBusMock.Verify(x => x.PublishAsync(It.IsAny<OrderPlacedIntegrationEvent>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        _dbMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task Handle_ShouldReturnFailure_WhenStockIsNotAvailable()
    {
        // Arrange
        var request = new PlaceOrderRequest
        {
            CustomerId = Guid.NewGuid(),
            ShippingAddress = new AddressDto { Street = "S", City = "C", Province = "P", ZipCode = "Z" },
            Lines = new List<OrderLineRequest> { new() { ProductId = Guid.NewGuid(), Quantity = 1 } }
        };
        var command = new PlaceOrderCommand(request, "tester");

        _inventoryServiceMock.Setup(x => x.CheckAvailabilityAsync(It.IsAny<StockCheckRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new StockCheckResponse(false, new List<StockCheckFailure> { new(Guid.NewGuid(), "Out of stock") }));

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Order.InsufficientStock");
    }
}
