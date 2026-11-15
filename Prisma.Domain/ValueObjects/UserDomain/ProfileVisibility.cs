using System.ComponentModel.DataAnnotations.Schema;

namespace Prisma.Domain.ValueObjects.UserDomain;

/// <summary>
/// What the teacher chose to show on the student-facing profile. Mapped by EF as a complex type.
/// </summary>
[ComplexType]
public sealed record ProfileVisibility
{
    public bool ShowYearsOfExperience { get; init; } = true;
    public bool ShowContactLinks { get; init; } = true;
    public bool ShowStudentsCount { get; init; } = true;
    public bool ShowLessonsCount { get; init; } = true;
    public bool ShowPopularLessons { get; init; } = true;
}
