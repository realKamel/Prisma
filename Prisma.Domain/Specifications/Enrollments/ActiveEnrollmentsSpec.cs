using Ardalis.Specification;
using Prisma.Domain.Entities.EnrollmentAggregate;
using Prisma.Domain.Enums;

namespace Prisma.Domain.Specifications.Enrollments;
public class ActiveEnrollmentsSpec : Specification<Enrollment>
{
    public ActiveEnrollmentsSpec(DateTimeOffset? before = null)
    {
        Query
            .Where(e => e.Status == EnrollmentStatus.Active
                     && (before == null || e.CreatedAt <= before))
            .AsNoTracking();
    }
}