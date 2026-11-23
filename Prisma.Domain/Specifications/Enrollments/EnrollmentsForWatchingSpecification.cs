using Ardalis.Specification;
using Prisma.Domain.Entities.EnrollmentAggregate;
using Prisma.Domain.Enums;

namespace Prisma.Domain.Specifications;

public sealed class EnrollmentsForWatchingSpecification : Specification<Enrollment>
{
    public EnrollmentsForWatchingSpecification(Guid studentId, int lessonId)
    {
        Query.Where(e =>
            e.StudentId == studentId
            && e.LessonId == lessonId
            && (e.Status == EnrollmentStatus.Active || e.Status == EnrollmentStatus.Done)
        );
    }
}