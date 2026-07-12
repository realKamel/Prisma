using Ardalis.Specification;
using Prisma.Domain.Entities.LessonAggregate;

namespace Prisma.Domain.Specifications.Lessons;
public sealed class LessonsSpec : Specification<Lesson>
{
    public LessonsSpec(DateTimeOffset? from = null)
    {
        Query
            .Where(l => from == null || l.CreatedAt >= from)
            .AsNoTracking();
    }
}