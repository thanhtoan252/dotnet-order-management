using Moq;
using FluentAssertions;
using NUnit.Framework;
using Shared.Core.CQRS;

namespace Shared.UnitTests.Core.CQRS;

[TestFixture]
public class DispatcherTests
{
    private Mock<IServiceProvider> _spMock;
    private Dispatcher _dispatcher;

    [SetUp]
    public void SetUp()
    {
        _spMock = new Mock<IServiceProvider>();
        _dispatcher = new Dispatcher(_spMock.Object);
    }

    [Test]
    public async Task SendAsync_Should_ResolveHandlerAndCallHandleAsync()
    {
        // Arrange
        var command = new TestCommand();
        var handlerMock = new Mock<ICommandHandler<TestCommand, bool>>();
        handlerMock.Setup(x => x.HandleAsync(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _spMock.Setup(x => x.GetService(typeof(ICommandHandler<TestCommand, bool>)))
            .Returns(handlerMock.Object);

        // Act
        var result = await _dispatcher.SendAsync(command, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        handlerMock.Verify(x => x.HandleAsync(command, It.IsAny<CancellationToken>()), Times.Once);
    }

    public class TestCommand : ICommand<bool> { }
}
