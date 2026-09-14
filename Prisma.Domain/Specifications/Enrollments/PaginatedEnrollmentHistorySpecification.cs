using System.Linq.Expressions;
using Ardalis.Specification;
using Prisma.Domain.Entities.EnrollmentAggregate;

namespace Prisma.Domain.Specifications.Enrollments;

public class PaginatedEnrollmentHistorySpecification<TSelector>
    : Specification<Enrollment, TSelector>
{
    public PaginatedEnrollmentHistorySpecification(
        Guid id,
        int skip,
        int take,
        Expression<Func<Enrollment, TSelector>> selector
    )
    {
        var skipAmount = (skip - 1) * take;
        Query.Where(s => s.StudentId == id)
            .AsNoTracking()
            .Skip(skipAmount < 0 ? 0 : skipAmount)
            .Take(take)
            .Select(selector);
    }
}
