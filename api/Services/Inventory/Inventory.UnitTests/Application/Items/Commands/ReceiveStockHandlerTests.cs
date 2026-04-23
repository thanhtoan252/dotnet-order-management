using Moq;
using FluentAssertions;
using NUnit.Framework;
using Microsoft.Extensions.Logging;
using Inventory.Application.Items.Commands;
using Inventory.Domain.Entities;
using Inventory.Domain.Repositories;
using Shared.Core.CQRS;
using Shared.Core.Domain;

namespace Inventory.UnitTests.Application.Items.Commands;

[TestFixture]
public class ReceiveStockHandlerTests
{
    private Mock<IInventoryRepository> _repoMock;
    private Mock<IUnitOfWork> _uowMock;
    private Mock<ILogger<ReceiveStockHandler>> _loggerMock;
    private ReceiveStockHandler _handler;

    [SetUp]
    public void SetUp()
    {
        _repoMock = new Mock<IInventoryRepository>();
        _uowMock = new Mock<IUnitOfWork>();
        _loggerMock = new Mock<ILogger<ReceiveStockHandler>>();
        _handler = new ReceiveStockHandler(_repoMock.Object, _uowMock.Object, _loggerMock.Object);
    }

    [Test]
    public async Task HandleAsync_Should_IncreaseOnHand_WhenItemFound()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var item = InventoryItem.Create(productId, "S", "N", 10).Value;

        _repoMock.Setup(x => x.GetByProductIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(item);

        // Act
        var result = await _handler.HandleAsync(new ReceiveStockCommand(productId, new ReceiveStockRequest { Quantity = 5 }), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        item.OnHand.Should().Be(15);
        _uowMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
