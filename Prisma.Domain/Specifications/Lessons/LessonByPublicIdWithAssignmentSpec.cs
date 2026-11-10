using Ardalis.Specification;
using Prisma.Domain.Entities.LessonAggregate;

namespace Prisma.Domain.Specifications.Lessons;

public sealed class LessonByPublicIdWithAssignmentSpec : Specification<Lesson>
{
    public LessonByPublicIdWithAssignmentSpec(Guid lessonId, bool tracking = false)
    {
        Query.Where(l => l.PublicId == lessonId).Include(x => x.Assignment);

        if (!tracking)
        {
            Query.AsNoTracking();
        }
    }
}
