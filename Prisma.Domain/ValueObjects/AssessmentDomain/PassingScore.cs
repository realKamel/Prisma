using Ardalis.Result;
using Prisma.Domain.ValueObjects.Common;

namespace Prisma.Domain.ValueObjects.AssessmentDomain;

public sealed record PassingScore : IComparable<PassingScore>
{
    public Percentage Value { get; init; } = Percentage.Zero;

    private PassingScore() { } // EF Core private constructor

    private PassingScore(Percentage percentage)
    {
        Value = percentage;
    }

    /// <summary>
    /// Factory method accepting primitive percentage value (0.00 to 100.00).
    /// </summary>
    public static Result<PassingScore> Create(decimal percentageValue)
    {
        var percentageResult = Percentage.Create(percentageValue);

        if (!percentageResult.IsSuccess)
        {
            return Result.Invalid(percentageResult.ValidationErrors);
        }

        return Result.Success(new PassingScore(percentageResult.Value));
    }

    /// <summary>
    /// Factory method accepting an existing Percentage Value Object.
    /// </summary>
    public static Result<PassingScore> Create(Percentage percentage)
    {
        ArgumentNullException.ThrowIfNull(percentage);
        return Result.Success(new PassingScore(percentage));
    }

    // Expressive Domain Factories
    public static PassingScore Standard50 => new(Percentage.Create(50m).Value);
    public static PassingScore Standard60 => new(Percentage.Create(60m).Value);
    public static PassingScore Standard75 => new(Percentage.Create(75m).Value);

    /// <summary>
    /// Evaluates if a given score or grade satisfies this passing requirement.
    /// </summary>
    public bool IsSatisfiedBy(Score score)
    {
        ArgumentNullException.ThrowIfNull(score);
        return score.Ratio * 100m >= Value.Value;
    }

    public bool IsSatisfiedBy(Percentage percentage)
    {
        ArgumentNullException.ThrowIfNull(percentage);
        return percentage.Value >= Value.Value;
    }

    // Implicit / Explicit Conversion Operators
    public static implicit operator Percentage(PassingScore passingScore)
    {
        ArgumentNullException.ThrowIfNull(passingScore);
        return passingScore.Value;
    }

    public static Percentage ToPercentage(PassingScore passingScore)
    {
        ArgumentNullException.ThrowIfNull(passingScore);
        return passingScore.Value;
    }

    public static explicit operator decimal(PassingScore passingScore)
    {
        ArgumentNullException.ThrowIfNull(passingScore);
        return passingScore.Value.Value;
    }

    public static decimal ToDecimal(PassingScore passingScore)
    {
        ArgumentNullException.ThrowIfNull(passingScore);
        return passingScore.Value.Value;
    }

    public int CompareTo(PassingScore? other)
    {
        return other is null ? 1 : Value.CompareTo(other.Value);
    }

    public override string ToString() => Value.ToString() ?? "0%";

    public static bool operator <(PassingScore? left, PassingScore? right)
    {
        return left is null ? right is not null : left.CompareTo(right) < 0;
    }

    public static bool operator <=(PassingScore? left, PassingScore? right)
    {
        return left is null || left.CompareTo(right) <= 0;
    }

    public static bool operator >(PassingScore? left, PassingScore? right)
    {
        return left is not null && left.CompareTo(right) > 0;
    }

    public static bool operator >=(PassingScore? left, PassingScore? right)
    {
        return left is null ? right is null : left.CompareTo(right) >= 0;
    }
}