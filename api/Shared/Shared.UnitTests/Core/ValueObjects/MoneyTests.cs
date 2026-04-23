using FluentAssertions;
using NUnit.Framework;
using Shared.Core.ValueObjects;

namespace Shared.UnitTests.Core.ValueObjects;

[TestFixture]
public class MoneyTests
{
    [Test]
    public void Create_Should_ReturnSuccess_WhenAmountIsPositive()
    {
        // Act
        var result = Money.Create(10, "USD");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Amount.Should().Be(10);
        result.Value.Currency.Should().Be("USD");
    }

    [Test]
    public void Create_Should_ReturnFailure_WhenAmountIsNegative()
    {
        // Act
        var result = Money.Create(-1, "USD");

        // Assert
        result.IsFailure.Should().BeTrue();
    }

    [Test]
    public void Addition_Should_SumAmounts_WhenCurrenciesAreSame()
    {
        // Arrange
        var m1 = Money.Create(10, "USD").Value;
        var m2 = Money.Create(20, "USD").Value;

        // Act
        var result = m1 + m2;

        // Assert
        result.Amount.Should().Be(30);
    }
}
