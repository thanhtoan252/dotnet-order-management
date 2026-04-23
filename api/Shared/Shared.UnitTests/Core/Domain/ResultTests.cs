using FluentAssertions;
using NUnit.Framework;
using Shared.Core.Domain;

namespace Shared.UnitTests.Core.Domain;

[TestFixture]
public class ResultTests
{
    [Test]
    public void Success_Should_ReturnIsSuccessTrue()
    {
        // Act
        var result = Result.Success();

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Test]
    public void Failure_Should_ReturnIsFailureTrue()
    {
        // Arrange
        var error = new Error("Code", "Message");

        // Act
        var result = Result.Failure(error);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(error);
    }
}
