using System.Linq.Expressions;
using Ardalis.Specification;
using Prisma.Domain.Entities.EnrollmentAggregate;

namespace Prisma.Domain.Specifications.Enrollments;

public class EnrollmentOrderByCreatedAtDesc<TResult> : Specification<Enrollment, TResult>
{
    public EnrollmentOrderByCreatedAtDesc(Guid id, Expression<Func<Enrollment, TResult>> selector)
    {
        Query
            .Where(x => x.StudentId == id)
            .OrderByDescending(p => p.CreatedAt)
            .AsSplitQuery()
            .AsNoTrackingWithIdentityResolution()
            .Select(selector);
    }
}
