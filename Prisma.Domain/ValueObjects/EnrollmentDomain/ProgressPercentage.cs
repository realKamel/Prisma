using Ardalis.Result;

namespace Prisma.Domain.ValueObjects.EnrollmentDomain;

public sealed record ProgressPercentage
{
    public int CompletedItems { get; init; }
    public int TotalItems { get; init; }

    /// <summary>
    /// Computed percentage rounded to 2 decimal places (0.00 to 100.00).
    /// </summary>
    public decimal Value => TotalItems == 0
        ? 0m
        : Math.Round((decimal)CompletedItems / TotalItems * 100m, 2);

    public bool IsCompleted => TotalItems > 0 && CompletedItems == TotalItems;

    private ProgressPercentage() { } // Required for EF Core materialization

    private ProgressPercentage(int completedItems, int totalItems)
    {
        CompletedItems = completedItems;
        TotalItems = totalItems;
    }

    /// <summary>
    /// Creates and validates a ProgressPercentage instance.
    /// </summary>
    public static Result<ProgressPercentage> Create(int completedItems, int totalItems)
    {
        var errors = new List<ValidationError>();

        if (totalItems < 0)
        {
            errors.Add(new ValidationError
            {
                Identifier = nameof(TotalItems), ErrorMessage = "Total items cannot be negative."
            });
        }

        if (completedItems < 0)
        {
            errors.Add(new ValidationError
            {
                Identifier = nameof(CompletedItems), ErrorMessage = "Completed items cannot be negative."
            });
        }
        else if (totalItems >= 0 && completedItems > totalItems)
        {
            errors.Add(new ValidationError
            {
                Identifier = nameof(CompletedItems),
                ErrorMessage = $"Completed items ({completedItems}) cannot exceed total items ({totalItems})."
            });
        }

        if (errors.Count > 0)
        {
            return Result.Invalid(errors);
        }

        return Result.Success(new ProgressPercentage(completedItems, totalItems));
    }

    // Expressive Domain Factories
    public static Result<ProgressPercentage> Zero(int totalItems = 0)
        => Create(0, totalItems);

    public static ProgressPercentage Empty => new(0, 0);

    // Immutable Domain Progression Methods
    public Result<ProgressPercentage> IncrementCompleted(int count = 1)
        => Create(CompletedItems + count, TotalItems);

    public Result<ProgressPercentage> UpdateTotal(int newTotal)
        => Create(CompletedItems, newTotal);

    // Explicit Conversion Operator (Safer than implicit to prevent hidden crashes)
    public static explicit operator decimal(ProgressPercentage progress)
    {
        ArgumentNullException.ThrowIfNull(progress);
        return progress.Value;
    }

    public static decimal ToDecimal(ProgressPercentage progress)
    {
        ArgumentNullException.ThrowIfNull(progress);
        return progress.Value;
    }

    public override string ToString() => $"{Value:0.##}%";
}