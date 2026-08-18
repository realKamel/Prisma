using Ardalis.Specification;
using Prisma.Domain.Entities.LessonAggregate;

namespace Prisma.Domain.Specifications.Lessons;

public class LessonPublicIdSpec : Specification<Lesson>
{
    public LessonPublicIdSpec(Guid id)
    {
        Query.Where(l => l.PublicId == id);
    }
}
