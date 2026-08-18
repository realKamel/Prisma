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
        Query.Where(s => s.StudentId == id).AsNoTracking().Skip(skip).Take(take).Select(selector);
    }
}
