using Ardalis.Specification;
using Prisma.Domain.Entities.EnrollmentAggregate;

namespace Prisma.Domain.Specifications.Admin;

public sealed class AdminLatestEnrollmentsSpec : Specification<Enrollment>
{
    public AdminLatestEnrollmentsSpec()
    {
        Query
            .Include(e => e.Student)
            .Include(e => e.Lesson)
            .OrderByDescending(e => e.CreatedAt)
            .Take(5);
    }
}