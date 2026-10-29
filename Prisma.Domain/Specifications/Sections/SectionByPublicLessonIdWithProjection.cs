using System.Linq.Expressions;
using Ardalis.Specification;
using Prisma.Domain.Entities.LessonAggregate;

namespace Prisma.Domain.Specifications.Sections;

public sealed class SectionByPublicLessonIdWithProjection<TResult> : Specification<Section, TResult>
{
    public SectionByPublicLessonIdWithProjection(
        Guid lessonId,
        Expression<Func<Section, TResult>> projection,
        bool tracking = false
    )
    {
        Query.Where(l => l.Lesson.PublicId == lessonId);

        if (!tracking)
        {
            Query.AsNoTracking();
        }

        Query.Select(projection);
    }

    public SectionByPublicLessonIdWithProjection(
        Guid publicSectionId,
        Guid publicLessonId,
        Expression<Func<Section, TResult>> projection,
        bool tracking = false
    )
    {
        Query.Where(l => l.PublicId == publicSectionId && l.Lesson.PublicId == publicLessonId);

        if (!tracking)
        {
            Query.AsNoTracking();
        }

        Query.Select(projection);
    }
}
