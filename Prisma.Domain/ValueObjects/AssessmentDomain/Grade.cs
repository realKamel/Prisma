using Ardalis.Result;
using Prisma.Domain.ValueObjects.Common;

namespace Prisma.Domain.ValueObjects.AssessmentDomain;

public sealed record Grade
{
    public Percentage Percentage { get; init; } = default!;

    private Grade() { } // EF Core private constructor

    private Grade(Percentage percentage)
    {
        Percentage = percentage;
    }

    public static Result<Grade> FromScore(Score score)
    {
        ArgumentNullException.ThrowIfNull(score, nameof(score));

        var percentageResult = score.ToPercentage();
        
        if (!percentageResult.IsSuccess)
        {
            return Result.Invalid(percentageResult.ValidationErrors);
        }

        return new Grade(percentageResult.Value);
    }

    public bool HasPassed(PassingScore passingThreshold)
    {
        ArgumentNullException.ThrowIfNull(passingThreshold, nameof(passingThreshold));
        return Percentage.Value >= passingThreshold.Value.Value;
    }

    public override string ToString() => Percentage.ToString();
}