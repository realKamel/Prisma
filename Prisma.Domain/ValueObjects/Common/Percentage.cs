using Ardalis.Result;

namespace Prisma.Domain.ValueObjects.Common;

public sealed record Percentage : IComparable<Percentage>
{
    public decimal Value { get; init; }

    private Percentage() { } // EF Core private constructor

    private Percentage(decimal value)
    {
        Value = value;
    }

    /// <summary>
    /// Factory method to create and validate a Percentage (0.00 to 100.00).
    /// </summary>
    public static Result<Percentage> Create(decimal value)
    {
        if (value is < 0m or > 100m)
        {
            return Result.Invalid(new ValidationError
            {
                Identifier = nameof(Value), ErrorMessage = $"Percentage must be between 0 and 100. Got: {value}"
            });
        }

        return Result.Success(new Percentage(Math.Round(value, 2)));
    }

    /// <summary>
    /// Creates a Percentage from a normalized fraction (e.g., 0.85 becomes 85%).
    /// </summary>
    public static Result<Percentage> FromFraction(decimal fraction)
        => Create(fraction * 100m);

    /// <summary>
    /// Creates a Percentage from completed vs total counts (e.g., 8 out of 10 = 80%).
    /// </summary>
    public static Result<Percentage> FromRatio(int completed, int total)
    {
        if (total <= 0)
        {
            return Result.Invalid(new ValidationError
            {
                Identifier = nameof(total), ErrorMessage = "Total count must be greater than zero."
            });
        }

        if (completed < 0 || completed > total)
        {
            return Result.Invalid(new ValidationError
            {
                Identifier = nameof(completed),
                ErrorMessage = $"Completed count ({completed}) cannot be negative or exceed total ({total})."
            });
        }

        decimal percentage = ((decimal)completed / total) * 100m;
        return Create(percentage);
    }

    // Expressive Domain Factories
    public static Percentage Zero => new(0m);
    public static Percentage Hundred => new(100m);

    /// <summary>
    /// Returns the decimal fraction equivalent (e.g., 85% returns 0.85).
    /// </summary>
    public decimal AsFraction => Value / 100m;

    /// <summary>
    /// Calculates the percentage portion of a given amount (e.g., 20% of 200 = 40).
    /// </summary>
    public decimal ApplyTo(decimal amount) => Math.Round(amount * AsFraction, 2);

    // Explicit Conversion Operator (Safer than implicit to prevent hidden null crashes)
    public static explicit operator decimal(Percentage percentage)
    {
        ArgumentNullException.ThrowIfNull(percentage);
        return percentage.Value;
    }

    public static decimal ToDecimal(Percentage percentage)
    {
        ArgumentNullException.ThrowIfNull(percentage);
        return percentage.Value;
    }

    // Comparison Operators
    public static bool operator >(Percentage a, Percentage b)
    {
        ArgumentNullException.ThrowIfNull(a);
        ArgumentNullException.ThrowIfNull(b);
        return a.Value > b.Value;
    }

    public static bool operator <(Percentage a, Percentage b)
    {
        ArgumentNullException.ThrowIfNull(a);
        ArgumentNullException.ThrowIfNull(b);
        return a.Value < b.Value;
    }

    public static bool operator >=(Percentage a, Percentage b)
    {
        ArgumentNullException.ThrowIfNull(a);
        ArgumentNullException.ThrowIfNull(b);
        return a.Value >= b.Value;
    }

    public static bool operator <=(Percentage a, Percentage b)
    {
        ArgumentNullException.ThrowIfNull(a);
        ArgumentNullException.ThrowIfNull(b);
        return a.Value <= b.Value;
    }

    public int CompareTo(Percentage? other)
    {
        return other is null ? 1 : Value.CompareTo(other.Value);
    }

    public override string ToString() => $"{Value:0.##}%";
}