using Ardalis.Result;

namespace Prisma.Domain.ValueObjects.EnrollmentDomain;

public sealed record DateRange
{
    public DateTimeOffset StartDate { get; init; }
    public DateTimeOffset EndDate { get; init; }

    public TimeSpan Duration => EndDate - StartDate;

    private DateRange(DateTimeOffset startDate, DateTimeOffset endDate)
    {
        StartDate = startDate;
        EndDate = endDate;
    }

    private DateRange() { } //for EF Core materialization

    /// <summary>
    /// Factory method to create and validate a DateRange.
    /// </summary>
    public static Result<DateRange> Create(DateTimeOffset startDate, DateTimeOffset endDate)
    {
        if (endDate < startDate)
        {
            return Result.Invalid(new ValidationError
            {
                Identifier = nameof(EndDate),
                ErrorMessage =
                    $"End date ({endDate:yyyy-MM-dd HH:mm:ss zzz}) cannot be earlier than start date ({startDate:yyyy-MM-dd HH:mm:ss zzz})."
            });
        }

        return Result.Success(new DateRange(startDate, endDate));
    }

    /// <summary>
    /// Creates a DateRange given a start date and duration.
    /// </summary>
    public static Result<DateRange> CreateFromDuration(DateTimeOffset startDate, TimeSpan duration)
    {
        if (duration < TimeSpan.Zero)
        {
            return Result.Invalid(new ValidationError
            {
                Identifier = nameof(duration), ErrorMessage = "Duration cannot be negative."
            });
        }

        return Create(startDate, startDate.Add(duration));
    }

    // Expressive Domain Factories
    public static Result<DateRange> StartingNow(TimeSpan duration)
        => CreateFromDuration(DateTimeOffset.UtcNow, duration);

    public bool Includes(DateTimeOffset dateTime) => dateTime >= StartDate && dateTime <= EndDate;

    public bool IsActiveAt(DateTimeOffset dateTime) => Includes(dateTime);

    public bool IsActiveNow() => Includes(DateTimeOffset.UtcNow);

    // Interval Operations
    public bool OverlapsWith(DateRange other)
    {
        ArgumentNullException.ThrowIfNull(other);
        return StartDate <= other.EndDate && EndDate >= other.StartDate;
    }

    /// <summary>
    /// Returns a new DateRange representing the intersection of two overlapping ranges.
    /// </summary>
    public Result<DateRange> Intersect(DateRange other)
    {
        ArgumentNullException.ThrowIfNull(other);

        if (!OverlapsWith(other))
        {
            return Result.Invalid(new ValidationError
            {
                Identifier = nameof(other), ErrorMessage = "Cannot intersect non-overlapping date ranges."
            });
        }

        var maxStart = StartDate > other.StartDate ? StartDate : other.StartDate;
        var minEnd = EndDate < other.EndDate ? EndDate : other.EndDate;

        return Create(maxStart, minEnd);
    }

    public override string ToString() => $"{StartDate:yyyy-MM-dd HH:mm:ss zzz} to {EndDate:yyyy-MM-dd HH:mm:ss zzz}";
};