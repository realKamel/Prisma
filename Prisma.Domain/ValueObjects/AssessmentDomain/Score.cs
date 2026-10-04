using Ardalis.Result;
using Prisma.Domain.ValueObjects.Common;

namespace Prisma.Domain.ValueObjects.AssessmentDomain;

public sealed record Score : IComparable<Score>
{
    public decimal AchievedPoints { get; init; }
    public decimal MaxPoints { get; init; }

    public decimal Ratio => MaxPoints == 0m ? 0m : AchievedPoints / MaxPoints;

    private Score() { } // EF Core private constructor

    private Score(decimal achievedPoints, decimal maxPoints)
    {
        AchievedPoints = achievedPoints;
        MaxPoints = maxPoints;
    }

    /// <summary>
    /// Factory method to create and validate an Assessment Score.
    /// </summary>
    public static Result<Score> Create(decimal achievedPoints, decimal maxPoints)
    {
        var errors = new List<ValidationError>();

        if (maxPoints <= 0)
        {
            errors.Add(new ValidationError
            {
                Identifier = nameof(MaxPoints), ErrorMessage = "Maximum points must be greater than 0."
            });
        }

        if (achievedPoints < 0)
        {
            errors.Add(new ValidationError
            {
                Identifier = nameof(AchievedPoints), ErrorMessage = "Achieved points cannot be negative."
            });
        }
        else if (maxPoints > 0 && achievedPoints > maxPoints)
        {
            errors.Add(new ValidationError
            {
                Identifier = nameof(AchievedPoints),
                ErrorMessage = $"Achieved points ({achievedPoints}) cannot exceed maximum points ({maxPoints})."
            });
        }

        if (errors.Count > 0)
        {
            return Result.Invalid(errors);
        }

        return Result.Success(new Score(Math.Round(achievedPoints, 2), Math.Round(maxPoints, 2)));
    }

    // Expressive Domain Factories
    public static Result<Score> Zero(decimal maxPoints) => Create(0m, maxPoints);
    public static Result<Score> Perfect(decimal maxPoints) => Create(maxPoints, maxPoints);

    /// <summary>
    /// Safely projects the score into a Percentage Value Object.
    /// </summary>
    public Result<Percentage> ToPercentage()
    {
        if (MaxPoints <= 0)
        {
            return Result.Invalid(new ValidationError
            {
                Identifier = nameof(MaxPoints),
                ErrorMessage = "Cannot calculate percentage for a score with zero or invalid maximum points."
            });
        }

        return Percentage.Create(Ratio * 100m);
    }

    /// <summary>
    /// Evaluates if the score satisfies a passing percentage threshold.
    /// </summary>
    public bool IsPassing(Percentage passingThreshold)
    {
        ArgumentNullException.ThrowIfNull(passingThreshold);
        return (Ratio * 100m) >= passingThreshold.Value;
    }

    // Arithmetic Operations for Combining Assessment Sections
    public static Result<Score> operator +(Score a, Score b)
    {
        ArgumentNullException.ThrowIfNull(a);
        ArgumentNullException.ThrowIfNull(b);

        return Create(a.AchievedPoints + b.AchievedPoints, a.MaxPoints + b.MaxPoints);
    }

    public static Result<Score> Add(Score a, Score b) => a + b;

    // Comparison Operators (Based on percentage/ratio, not just raw points)
    public static bool operator >(Score a, Score b)
    {
        ArgumentNullException.ThrowIfNull(a);
        ArgumentNullException.ThrowIfNull(b);
        return a.Ratio > b.Ratio;
    }

    public static bool operator <(Score a, Score b)
    {
        ArgumentNullException.ThrowIfNull(a);
        ArgumentNullException.ThrowIfNull(b);
        return a.Ratio < b.Ratio;
    }

    public static bool operator >=(Score a, Score b)
    {
        ArgumentNullException.ThrowIfNull(a);
        ArgumentNullException.ThrowIfNull(b);
        return a.Ratio >= b.Ratio;
    }

    public static bool operator <=(Score a, Score b)
    {
        ArgumentNullException.ThrowIfNull(a);
        ArgumentNullException.ThrowIfNull(b);
        return a.Ratio <= b.Ratio;
    }

    public int CompareTo(Score? other)
    {
        if (other is null) return 1;
        return Ratio.CompareTo(other.Ratio);
    }

    public override string ToString() => $"{AchievedPoints:0.##} / {MaxPoints:0.##}";
}