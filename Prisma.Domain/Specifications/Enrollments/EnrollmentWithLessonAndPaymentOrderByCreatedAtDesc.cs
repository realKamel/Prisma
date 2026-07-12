using System.Linq.Expressions;
using Ardalis.Specification;
using Prisma.Domain.Entities.EnrollmentAggregate;

namespace Prisma.Domain.Specifications.Enrollments;

public class EnrollmentWithLessonAndPaymentOrderByCreatedAtDesc : Specification<Enrollment>
{
    public EnrollmentWithLessonAndPaymentOrderByCreatedAtDesc(Expression<Func<Enrollment, bool>> expression)
    {
        Query.Where(expression).Include(e => e.Lesson).Include(e => e.Payment)
            .OrderByDescending(p => p.CreatedAt)
            .AsSplitQuery().AsNoTrackingWithIdentityResolution();
    }
}