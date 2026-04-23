using FluentAssertions;
using NUnit.Framework;
using Shared.Core.ValueObjects;

namespace Shared.UnitTests.ValueObjects;

public class MoneyTests
{
    [Test]
    public void Create_ShouldReturnSuccess_WhenAmountIsPositive()
    {
        // Act
        var result = Money.Create(100, "USD");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Amount.Should().Be(100);
        result.Value.Currency.Should().Be("USD");
    }

    [Test]
    public void Create_ShouldReturnFailure_WhenAmountIsNegative()
    {
        // Act
        var result = Money.Create(-1, "USD");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Money.NegativeAmount");
    }

    [Test]
    public void Create_ShouldReturnFailure_WhenCurrencyIsEmpty()
    {
        // Act
        var result = Money.Create(100, "");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Money.InvalidCurrency");
    }

    [Test]
    public void Add_ShouldSumAmounts_WhenCurrenciesMatch()
    {
        // Arrange
        var m1 = Money.Create(100, "USD").Value;
        var m2 = Money.Create(50, "USD").Value;

        // Act
        var result = m1 + m2;

        // Assert
        result.Amount.Should().Be(150);
        result.Currency.Should().Be("USD");
    }

    [Test]
    public void Add_ShouldThrow_WhenCurrenciesDoNotMatch()
    {
        // Arrange
        var m1 = Money.Create(100, "USD").Value;
        var m2 = Money.Create(50, "EUR").Value;

        // Act
        Action act = () => { _ = m1 + m2; };

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*different currencies*");
    }
}
