namespace Prisma.Domain.ValueObjects;

public record Money(decimal Amount, string Currency = "EGP")
{
    private Money()
        : this(0, "EGP") { } // EF Core

    public static Money operator -(Money m)
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

    public static Money operator -(Money a, Money b)
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
}
