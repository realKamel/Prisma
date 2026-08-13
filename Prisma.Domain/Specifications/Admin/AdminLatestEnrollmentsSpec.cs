using Ardalis.Specification;
using Prisma.Domain.Entities.EnrollmentAggregate;
using Prisma.Domain.Enums;

namespace Prisma.Domain.Specifications.Admin;

public sealed class AdminLatestEnrollmentsSpec : Specification<Enrollment, EnrollmentActivityProjection>
{
    public AdminLatestEnrollmentsSpec()
    {
        Query
            .OrderByDescending(e => e.CreatedAt)
            .Take(5)
            .Select(e => new EnrollmentActivityProjection(
                e.Id,
                e.StudentId,
                e.Lesson!.Title,
                e.EnrollmentMethod,
                e.CreatedAt
            ));
    }
}
public sealed record EnrollmentActivityProjection(
    int Id,
    Guid? StudentId,
    string? LessonTitle,
    EnrollmentMethod EnrollmentMethod,
    DateTimeOffset? CreatedAt
);