using Moq;
using FluentAssertions;
using NUnit.Framework;
using Inventory.Application.Items.Queries;
using Inventory.Domain.Entities;
using Inventory.Domain.Repositories;
using Shared.Contracts;

namespace Inventory.UnitTests.Application.Items.Queries;

[TestFixture]
public class CheckAvailabilityHandlerTests
{
    private Mock<IInventoryRepository> _repoMock;
    private CheckAvailabilityHandler _handler;

    [SetUp]
    public void SetUp()
    {
        _repoMock = new Mock<IInventoryRepository>();
        _handler = new CheckAvailabilityHandler(_repoMock.Object);
    }

    [Test]
    public async Task HandleAsync_Should_ReturnSuccess_WhenStockIsAvailable()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var item = InventoryItem.Create(productId, "S", "N", 10).Value;
        _repoMock.Setup(x => x.GetByProductIdsAsync(It.IsAny<List<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<InventoryItem> { item });

        var query = new CheckAvailabilityQuery(new List<StockCheckItem> { new(productId, 5) });

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.IsAvailable.Should().BeTrue();
        result.Failures.Should().BeEmpty();
    }

    [Test]
    public async Task HandleAsync_Should_ReturnFailure_WhenStockIsInsufficient()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var item = InventoryItem.Create(productId, "S", "N", 10).Value;
        _repoMock.Setup(x => x.GetByProductIdsAsync(It.IsAny<List<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<InventoryItem> { item });

        var query = new CheckAvailabilityQuery(new List<StockCheckItem> { new(productId, 15) });

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.IsAvailable.Should().BeFalse();
        result.Failures.Should().HaveCount(1);
    }
}
