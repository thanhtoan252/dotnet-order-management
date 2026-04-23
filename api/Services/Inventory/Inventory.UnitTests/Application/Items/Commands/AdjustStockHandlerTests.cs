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
public class AdjustStockHandlerTests
{
    private Mock<IInventoryRepository> _repoMock;
    private Mock<IUnitOfWork> _uowMock;
    private Mock<ILogger<AdjustStockHandler>> _loggerMock;
    private AdjustStockHandler _handler;

    [SetUp]
    public void SetUp()
    {
        _repoMock = new Mock<IInventoryRepository>();
        _uowMock = new Mock<IUnitOfWork>();
        _loggerMock = new Mock<ILogger<AdjustStockHandler>>();
        _handler = new AdjustStockHandler(_repoMock.Object, _uowMock.Object, _loggerMock.Object);
    }

    [Test]
    public async Task HandleAsync_Should_UpdateOnHand_WhenItemFound()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var item = InventoryItem.Create(productId, "S", "N", 10).Value;

        _repoMock.Setup(x => x.GetByProductIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(item);

        // Act
        var result = await _handler.HandleAsync(new AdjustStockCommand(productId, new AdjustStockRequest { OnHand = 20 }), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        item.OnHand.Should().Be(20);
        _uowMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
