using Ardalis.Specification;
using Prisma.Domain.Entities.LessonAggregate;

namespace Prisma.Domain.Specifications.Lessons;

public sealed class LessonByPublicId : Specification<Lesson>
{
    public LessonByPublicId(Guid lessonId, bool tracking = false)
    {
        Query.Where(l => l.PublicId == lessonId);

        if (!tracking)
        {
            Query.AsNoTracking();
        }
    }
}
