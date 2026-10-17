using Ardalis.Result;

namespace Prisma.Domain.ValueObjects.ContentDomain;

public sealed record TimeDuration : IComparable<TimeDuration>
{
    public long Seconds { get; init; }

    public TimeSpan Value => TimeSpan.FromSeconds(Seconds);

    private TimeDuration() { } // EF Core private constructor

    private TimeDuration(long seconds)
    {
        Seconds = seconds;
    }

    /// <summary>
    /// Generic/default factory method accepting total seconds.
    /// </summary>
    public static Result<TimeDuration> Create(long seconds) => FromSeconds(seconds);

    public static Result<TimeDuration> Create(double seconds)
    {
        if (double.IsNaN(seconds) || double.IsInfinity(seconds))
        {
            return Result.Invalid(
                new ValidationError
                {
                    Identifier = nameof(seconds),
                    ErrorMessage = "Duration must be a valid number.",
                }
            );
        }

        return FromSeconds((long)Math.Round(seconds, MidpointRounding.AwayFromZero));
    }

    /// <summary>
    /// Factory method accepting total seconds.
    /// </summary>
    public static Result<TimeDuration> FromSeconds(long seconds) => ValidateAndCreate(seconds);

    /// <summary>
    /// Factory method accepting TimeSpan with overflow protection.
    /// </summary>
    public static Result<TimeDuration> FromTimeSpan(TimeSpan timeSpan)
    {
        if (timeSpan < TimeSpan.Zero)
        {
            return Result.Invalid(
                new ValidationError
                {
                    Identifier = nameof(timeSpan),
                    ErrorMessage = "Duration cannot be negative.",
                }
            );
        }

        if (timeSpan.TotalSeconds > long.MaxValue)
        {
            return Result.Invalid(
                new ValidationError
                {
                    Identifier = nameof(timeSpan),
                    ErrorMessage =
                        $"Duration exceeds maximum supported limit of {long.MaxValue} seconds.",
                }
            );
        }

        return Result.Success(new TimeDuration((long)Math.Round(timeSpan.TotalSeconds)));
    }

    public static TimeDuration Zero => new(0);

    public static Result<TimeDuration> FromMinutes(long minutes)
    {
        long totalSeconds = minutes * 60;
        return ValidateAndCreate(totalSeconds);
    }

    public static Result<TimeDuration> FromHours(long hours)
    {
        long totalSeconds = hours * 3600;
        return ValidateAndCreate(totalSeconds);
    }

    // --- Safe Arithmetic Methods (Returning Result) ---

    public static Result<TimeDuration> Add(TimeDuration? left, TimeDuration? right)
    {
        ArgumentNullException.ThrowIfNull(left);
        ArgumentNullException.ThrowIfNull(right);

        var sum = left.Seconds + right.Seconds;
        return ValidateAndCreate(sum, "Combined duration exceeds maximum supported limit.");
    }

    public static Result<TimeDuration> Subtract(TimeDuration? left, TimeDuration? right)
    {
        ArgumentNullException.ThrowIfNull(left);
        ArgumentNullException.ThrowIfNull(right);

        var result = left.Seconds - right.Seconds;
        if (result < 0)
        {
            return Result.Invalid(
                new ValidationError
                {
                    Identifier = nameof(TimeDuration),
                    ErrorMessage = "Resulting duration cannot be negative.",
                }
            );
        }

        return Result.Success(new TimeDuration(result));
    }

    // --- Explicit Casts & Helpers ---

    public static explicit operator long(TimeDuration duration)
    {
        ArgumentNullException.ThrowIfNull(duration);
        return duration.Seconds;
    }

    public static long ToInt64(TimeDuration duration)
    {
        ArgumentNullException.ThrowIfNull(duration);
        return duration.Seconds;
    }

    public override string ToString() => Seconds.ToString();

    public int CompareTo(TimeDuration? other)
    {
        return other is null ? 1 : Seconds.CompareTo(other.Seconds);
    }

    private static Result<TimeDuration> ValidateAndCreate(
        long totalSeconds,
        string overflowMessage = "Duration exceeds maximum supported limit."
    )
    {
        return totalSeconds switch
        {
            < 0 => Result.Invalid(
                new ValidationError
                {
                    Identifier = nameof(TimeDuration),
                    ErrorMessage = "Duration seconds cannot be negative.",
                }
            ),
            >= long.MaxValue => Result.Invalid(
                new ValidationError
                {
                    Identifier = nameof(TimeDuration),
                    ErrorMessage = overflowMessage,
                }
            ),
            _ => Result.Success(new TimeDuration(totalSeconds)),
        };
    }

    //Comparison Operators

    public static bool operator <(TimeDuration? left, TimeDuration? right)
    {
        return left is not null && right is not null && left.Value.CompareTo(right.Value) < 0;
    }

    public static bool operator <=(TimeDuration? left, TimeDuration? right)
    {
        return left is not null && right is not null && left.Value.CompareTo(right.Value) <= 0;
    }

    public static bool operator >(TimeDuration? left, TimeDuration? right)
    {
        return left is not null && right is not null && left.Value.CompareTo(right.Value) > 0;
    }

    public static bool operator >=(TimeDuration? left, TimeDuration? right)
    {
        return left is not null && right is not null && left.Value.CompareTo(right.Value) >= 0;
    }
}
