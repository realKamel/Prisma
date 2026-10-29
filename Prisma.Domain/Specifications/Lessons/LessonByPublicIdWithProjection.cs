using System.Linq.Expressions;
using Ardalis.Specification;
using Prisma.Domain.Entities.LessonAggregate;

namespace Prisma.Domain.Specifications.Lessons;

public sealed class LessonByPublicIdWithProjection<TResult> : Specification<Lesson, TResult>
{
    public LessonByPublicIdWithProjection(
        Guid lessonId,
        Expression<Func<Lesson, TResult>> expression,
        bool tracking = false
    )
    {
        Query.Where(l => l.PublicId == lessonId);

        if (!tracking)
        {
            Query.AsNoTracking();
        }

        Query.AsSplitQuery().Select(expression);
    }
}
