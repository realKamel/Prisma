using global::Prisma.Domain.Entities.EnrollmentAggregate;
using Ardalis.Specification;

namespace Prisma.Domain.Specifications.AdminDashboard;

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