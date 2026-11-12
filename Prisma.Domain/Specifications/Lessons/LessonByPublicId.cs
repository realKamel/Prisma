using Ardalis.Specification;
using Prisma.Domain.Entities.LessonAggregate;

namespace Prisma.Domain.Specifications.Lessons;

public sealed class LessonByPublicId : Specification<Lesson>
{
    public LessonByPublicId(Guid lessonId, bool asNoTracking = false)
    {
        Query
            .Where(l => l.PublicId == lessonId)
            .Include(x => x.Sections)
            .Include(l => l.Assignment)
            .AsSplitQuery();

        if (asNoTracking)
        {
            Query.AsNoTracking();
        }
    }
}
