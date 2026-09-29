namespace Prisma.Domain.ValueObjects;

public sealed record Money : IComparable<Money>
{
    public decimal Amount { get; init; }
    public string Currency { get; init; }

    public Money(decimal amount, string currency = "EGP")
    {
        if (string.IsNullOrWhiteSpace(currency) || currency.Trim().Length != 3)
        {
            throw new ArgumentException("Currency must be a 3-letter ISO code.", nameof(currency));
        }

        Amount = amount;
        Currency = currency.Trim().ToUpperInvariant();
    }

    private Money()
        : this(0, "EGP")
    {
    } // EF Core

    public static Money operator -(Money m)
    {
        ArgumentNullException.ThrowIfNull(m);
        return m with { Amount = -m.Amount };
    }

    public static Money Negate(Money m)
    {
        ArgumentNullException.ThrowIfNull(m);
        return m with { Amount = -m.Amount };
    }

    //Addition & Subtraction
    public static Money operator +(Money a, Money b)
    {
        EnsureSameCurrency(a, b);
        return a with { Amount = a.Amount + b.Amount };
    }

    public static Money Add(Money a, Money b)
    {
        EnsureSameCurrency(a, b);
        return a with { Amount = a.Amount + b.Amount };
    }

    public static Money operator -(Money a, Money b)
    {
        EnsureSameCurrency(a, b);
        return a with { Amount = a.Amount - b.Amount };
    }

    public static Money Subtract(Money a, Money b)
    {
        EnsureSameCurrency(a, b);
        return a with { Amount = a.Amount - b.Amount };
    }

    // Multiplication & Division
    public static Money operator *(Money m, decimal factor)
    {
        ArgumentNullException.ThrowIfNull(m);
        return m with { Amount = m.Amount * factor };
    }

    public static Money Multiply(Money m, decimal factor)
    {
        ArgumentNullException.ThrowIfNull(m);
        return m with { Amount = m.Amount * factor };
    }

    public static Money operator *(decimal factor, Money m) => m * factor;

    public static Money operator /(Money m, decimal divisor)
    {
        ArgumentNullException.ThrowIfNull(m);
        if (divisor == 0)
        {
            throw new DivideByZeroException("Cannot divide Money by zero.");
        }

        return m with { Amount = m.Amount / divisor };
    }

    public static Money Divide(Money m, decimal divisor)
    {
        ArgumentNullException.ThrowIfNull(m);
        if (divisor == 0)
        {
            throw new DivideByZeroException("Cannot divide Money by zero.");
        }

        return m with { Amount = m.Amount / divisor };
    }

    //Comparison Operators
    public static bool operator >(Money a, Money b)
    {
        EnsureSameCurrency(a, b);
        return a.Amount > b.Amount;
    }

    public static bool operator <(Money a, Money b)
    {
        EnsureSameCurrency(a, b);
        return a.Amount < b.Amount;
    }

    public static bool operator >=(Money a, Money b)
    {
        EnsureSameCurrency(a, b);
        return a.Amount >= b.Amount;
    }

    public static bool operator <=(Money a, Money b)
    {
        EnsureSameCurrency(a, b);
        return a.Amount <= b.Amount;
    }

    // Helper method to guard currency invariants
    private static void EnsureSameCurrency(Money a, Money b)
    {
        ArgumentNullException.ThrowIfNull(a);
        ArgumentNullException.ThrowIfNull(b);

        if (a.Currency != b.Currency)
        {
            throw new InvalidOperationException(
                $"Currency mismatch: Cannot operate on {a.Currency} and {b.Currency}."
            );
        }
    }


    public int CompareTo(Money? other)
    {
        if (other is null)
        {
            return 1;
        }

        EnsureSameCurrency(this, other);
        return Amount.CompareTo(other.Amount);
    }
}