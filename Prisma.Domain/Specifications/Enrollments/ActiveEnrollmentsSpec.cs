using Ardalis.Specification;
using Prisma.Domain.Entities.EnrollmentAggregate;
using Prisma.Domain.Enums;

namespace Prisma.Domain.Specifications.Enrollments;

public sealed class ActiveEnrollmentsSpec : Specification<Enrollment>
{
    public ActiveEnrollmentsSpec(Guid teacherId, DateTimeOffset? before = null)
    {
        Query
            .Where(e => e.Status == EnrollmentStatus.Active
                     && e.Lesson!.TeacherId == teacherId
                     && (before == null || e.CreatedAt <= before))
            .AsNoTracking();
    }
}