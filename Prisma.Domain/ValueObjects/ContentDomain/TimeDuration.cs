using Ardalis.Result;

namespace Prisma.Domain.ValueObjects.ContentDomain;

public sealed record TimeDuration : IComparable<TimeDuration>
{
    public int Seconds { get; init; }

    public TimeSpan Value => TimeSpan.FromSeconds(Seconds);

    private TimeDuration() { } // EF Core private constructor

    private TimeDuration(int seconds)
    {
        Seconds = seconds;
    }

    /// <summary>
    /// Factory method accepting total seconds.
    /// </summary>
    public static Result<TimeDuration> FromSeconds(int seconds)
    {
        if (seconds < 0)
        {
            return Result.Invalid(new ValidationError
            {
                Identifier = nameof(Seconds), ErrorMessage = "Duration seconds cannot be negative."
            });
        }

        return Result.Success(new TimeDuration(seconds));
    }

    /// <summary>
    /// Factory method accepting TimeSpan with overflow protection.
    /// </summary>
    public static Result<TimeDuration> FromTimeSpan(TimeSpan timeSpan)
    {
        if (timeSpan < TimeSpan.Zero)
        {
            return Result.Invalid(new ValidationError
            {
                Identifier = nameof(timeSpan), ErrorMessage = "Duration cannot be negative."
            });
        }

        if (timeSpan.TotalSeconds > int.MaxValue)
        {
            return Result.Invalid(new ValidationError
            {
                Identifier = nameof(timeSpan),
                ErrorMessage = $"Duration exceeds maximum supported limit of {int.MaxValue} seconds."
            });
        }

        return Result.Success(new TimeDuration((int)Math.Round(timeSpan.TotalSeconds)));
    }

    public static TimeDuration Zero => new(0);
    public static Result<TimeDuration> FromMinutes(int minutes) => FromSeconds(minutes * 60);
    public static Result<TimeDuration> FromHours(int hours) => FromSeconds(hours * 3600);

    // Arithmetic operators for aggregating course sections
    public static Result<TimeDuration> operator +(TimeDuration left, TimeDuration right)
    {
        ArgumentNullException.ThrowIfNull(left);
        ArgumentNullException.ThrowIfNull(right);

        long sum = (long)left.Seconds + right.Seconds;
        if (sum > int.MaxValue)
        {
            return Result.Invalid(new ValidationError
            {
                Identifier = nameof(TimeDuration),
                ErrorMessage = "Combined duration exceeds maximum supported limit."
            });
        }

        return Result.Success(new TimeDuration((int)sum));
    }

    public static Result<TimeDuration> Add(TimeDuration left, TimeDuration right) => left + right;

    public static Result<TimeDuration> operator -(TimeDuration left, TimeDuration right)
    {
        ArgumentNullException.ThrowIfNull(left);
        ArgumentNullException.ThrowIfNull(right);

        int result = left.Seconds - right.Seconds;
        if (result < 0)
        {
            return Result.Invalid(new ValidationError
            {
                Identifier = nameof(TimeDuration), ErrorMessage = "Resulting duration cannot be negative."
            });
        }

        return Result.Success(new TimeDuration(result));
    }

    public static Result<TimeDuration> Subtract(TimeDuration left, TimeDuration right) => left - right;

    public static explicit operator int(TimeDuration duration)
    {
        ArgumentNullException.ThrowIfNull(duration);
        return duration.Seconds;
    }

    public static int ToInt32(TimeDuration duration)
    {
        ArgumentNullException.ThrowIfNull(duration);
        return duration.Seconds;
    }

    // Returns unformatted raw seconds as string (e.g. "450" or "3600")
    public override string ToString() => Seconds.ToString();

    public int CompareTo(TimeDuration? other)
    {
        return other is null ? 1 : Seconds.CompareTo(other.Seconds);
    }
}