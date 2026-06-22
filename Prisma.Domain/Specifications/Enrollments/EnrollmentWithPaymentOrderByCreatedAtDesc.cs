using System.Linq.Expressions;
using Ardalis.Specification;

namespace Prisma.Domain.Specifications.Enrollments;

public class EnrollmentWithPaymentOrderByCreatedAtDesc : Specification<Entities.EnrollmentAggregate.Enrollment>
{
    public EnrollmentWithPaymentOrderByCreatedAtDesc(
        Expression<Func<Entities.EnrollmentAggregate.Enrollment, bool>> expression)
    {
        Query.Where(expression).Include(e => e.Payment)
            .OrderByDescending(p => p.CreatedAt)
            .AsNoTrackingWithIdentityResolution();
    }
}