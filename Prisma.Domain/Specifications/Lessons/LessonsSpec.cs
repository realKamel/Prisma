using Ardalis.Specification;
using Prisma.Domain.Entities.LessonAggregate;

namespace Prisma.Domain.Specifications.Lessons;

public sealed class LessonsSpec : Specification<Lesson>
{
    public LessonsSpec(Guid teacherId, DateTimeOffset? from = null)
    {
        Query
            .Where(l => l.TeacherId == teacherId
                     && (from == null || l.CreatedAt >= from))
            .AsNoTracking();
    }
}
