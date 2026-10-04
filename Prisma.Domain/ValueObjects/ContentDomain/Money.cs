using Ardalis.Result;

namespace Prisma.Domain.ValueObjects.ContentDomain;

public sealed record Money : IComparable<Money>
{
    public decimal Amount { get; init; }
    public string Currency { get; init; } = default!;

    private Money() { } // EF Core private constructor

    private Money(decimal amount, string currency)
    {
        Amount = amount;
        Currency = currency;
    }

    /// <summary>
    /// Factory method to create and validate a Money instance.
    /// </summary>
    public static Result<Money> Create(decimal amount, string currency = "EGP", bool allowNegative = false)
    {
        var errors = new List<ValidationError>();

        if (!allowNegative && amount < 0)
        {
            errors.Add(new ValidationError
            {
                Identifier = nameof(Amount), ErrorMessage = "Money amount cannot be negative."
            });
        }

        if (string.IsNullOrWhiteSpace(currency) || currency.Trim().Length != 3)
        {
            errors.Add(new ValidationError
            {
                Identifier = nameof(Currency),
                ErrorMessage = "Currency must be a valid 3-letter ISO code (e.g., EGP, USD)."
            });
        }

        if (errors.Count > 0)
        {
            return Result.Invalid(errors);
        }

        string formattedCurrency = currency.Trim().ToUpperInvariant();

        return Result.Success(new Money(amount, formattedCurrency));
    }

    // Expressive Domain Factories
    public static Result<Money> Zero(string currency = "EGP") => Create(0m, currency);
    public static Result<Money> Egp(decimal amount) => Create(amount, "EGP");
    public static Result<Money> Usd(decimal amount) => Create(amount, "USD");

    // Unary Operators
    // Unary Operators
    public static Money operator -(Money m)
    {
        ArgumentNullException.ThrowIfNull(m);
        return m with { Amount = -m.Amount };
    }

    public static Money Negate(Money m) => -m;

    // Addition & Subtraction
    public static Money operator +(Money a, Money b)
    {
        EnsureSameCurrency(a, b);
        return a with { Amount = a.Amount + b.Amount };
    }

    public static Money Add(Money a, Money b) => a + b;

    public static Money operator -(Money a, Money b)
    {
        EnsureSameCurrency(a, b);
        return a with { Amount = a.Amount - b.Amount };
    }

    public static Money Subtract(Money a, Money b) => a - b;

    // Multiplication & Division
    public static Money operator *(Money m, decimal factor)
    {
        ArgumentNullException.ThrowIfNull(m);
        return m with { Amount = m.Amount * factor };
    }

    public static Money Multiply(Money m, decimal factor) => m * factor;

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

    public static Money Divide(Money m, decimal divisor) => m / divisor;

    // Comparison Operators
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

    public int CompareTo(Money? other)
    {
        if (other is null)
        {
            return 1;
        }

        EnsureSameCurrency(this, other);
        return Amount.CompareTo(other.Amount);
    }

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

    public override string ToString() => $"{Amount:N2} {Currency}";
}