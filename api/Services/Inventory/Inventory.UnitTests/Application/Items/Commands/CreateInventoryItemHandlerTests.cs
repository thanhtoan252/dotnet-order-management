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
public class CreateInventoryItemHandlerTests
{
    private Mock<IInventoryRepository> _repoMock;
    private Mock<IUnitOfWork> _uowMock;
    private Mock<ILogger<CreateInventoryItemHandler>> _loggerMock;
    private CreateInventoryItemHandler _handler;

    [SetUp]
    public void SetUp()
    {
        _repoMock = new Mock<IInventoryRepository>();
        _uowMock = new Mock<IUnitOfWork>();
        _loggerMock = new Mock<ILogger<CreateInventoryItemHandler>>();

        _handler = new CreateInventoryItemHandler(
            _repoMock.Object,
            _uowMock.Object,
            _loggerMock.Object);
    }

    [Test]
    public async Task HandleAsync_Should_ReturnSuccess_WhenDataIsValid()
    {
        // Arrange
        var request = new CreateInventoryItemRequest
        {
            ProductId = Guid.NewGuid(),
            Sku = "SKU",
            ProductName = "Product",
            InitialQuantity = 10
        };
        var command = new CreateInventoryItemCommand(request);

        _repoMock.Setup(x => x.ExistsForProductAsync(request.ProductId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _repoMock.Verify(x => x.Add(It.IsAny<InventoryItem>()), Times.Once);
        _uowMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
