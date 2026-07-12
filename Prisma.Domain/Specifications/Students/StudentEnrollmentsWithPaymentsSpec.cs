using Ardalis.Specification;
using Prisma.Domain.Entities.EnrollmentAggregate;

namespace Prisma.Domain.Specifications.Students;

public sealed class StudentEnrollmentsWithPaymentsSpec : Specification<Enrollment>
{
    public StudentEnrollmentsWithPaymentsSpec(Guid studentId)
    {
        Query.Where(e => e.StudentId == studentId)
       .Include(e => e.Lesson)
       .Include(e => e.Payment)
       .OrderByDescending(e => e.CreatedAt)
       .AsNoTracking();
    }
}