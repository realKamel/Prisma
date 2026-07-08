using System.Linq.Expressions;
using Ardalis.Specification;
using Prisma.Domain.Entities.EnrollmentAggregate;

namespace Prisma.Domain.Specifications.Enrollments;

public class EnrollmentAndLessonAndLessonProgressesOrderByCreatedAtDesc<TResult> :
    Specification<Enrollment, TResult>
{
    public EnrollmentAndLessonAndLessonProgressesOrderByCreatedAtDesc
    (Expression<Func<Enrollment, bool>>
        expression, Expression<Func<Enrollment, TResult>> selector)
    {
        Query.Where(expression)
            .Include(e => e.Lesson)
            .ThenInclude(l => l.Sections)
            .ThenInclude(s => s.Progresses)
            .Include(e => e.Lesson)
            .ThenInclude(l => l.Assignment)
            .OrderByDescending(p => p.CreatedAt)
            .AsSplitQuery()
            .AsNoTrackingWithIdentityResolution()
            .Select(selector);
    }
}