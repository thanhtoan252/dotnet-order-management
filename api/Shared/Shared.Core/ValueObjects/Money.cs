namespace Shared.Core.ValueObjects;

/// <summary>
///     Immutable value object — represents a monetary amount with currency.
/// </summary>
public sealed record Money(decimal Amount, string Currency = "USD")
{
    public static Money Zero(string currency = "USD")
    {
        return new Money(0, currency);
    }

    public static Money Create(decimal amount, string currency = "USD")
    {
        if (amount < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(amount), "Amount cannot be negative.");
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(currency);

        return new Money(amount, currency.ToUpperInvariant());
    }

    public static Money operator +(Money a, Money b)
    {
        EnsureSameCurrency(a, b);

        return new Money(a.Amount + b.Amount, a.Currency);
    }

    public static Money operator -(Money a, Money b)
    {
        EnsureSameCurrency(a, b);
        if (a.Amount < b.Amount)
        {
            throw new InvalidOperationException("Result cannot be negative.");
        }

        return new Money(a.Amount - b.Amount, a.Currency);
    }

    public static Money operator *(Money money, int multiplier)
    {
        return new Money(money.Amount * multiplier, money.Currency);
    }

    private static void EnsureSameCurrency(Money a, Money b)
    {
        if (a.Currency != b.Currency)
        {
            throw new InvalidOperationException(
                $"Cannot operate on different currencies: {a.Currency} vs {b.Currency}.");
        }
    }

    public override string ToString()
    {
        return $"{Amount:N0} {Currency}";
    }
}
