using System.Linq.Expressions;
using Ardalis.Specification;
using Prisma.Domain.Entities.EnrollmentAggregate;

namespace Prisma.Domain.Specifications.Enrollments;

public class EnrollmentWithPaymentOrderByCreatedAtDesc : Specification<Enrollment>
{
    public EnrollmentWithPaymentOrderByCreatedAtDesc(Expression<Func<Enrollment, bool>> expression)
    {
        Query
            .Where(expression)
            .Include(e => e.Payment)
            .OrderByDescending(p => p.CreatedAt)
            .AsNoTrackingWithIdentityResolution();
    }
}
