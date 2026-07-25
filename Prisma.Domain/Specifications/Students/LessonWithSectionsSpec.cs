using Ardalis.Specification;
using Prisma.Domain.Entities.LessonAggregate;

namespace Prisma.Domain.Specifications.Students;

public class LessonWithSectionsSpec : Specification<Lesson>
{
    public LessonWithSectionsSpec(int lessonId)
    {
        Query
            .AsNoTracking()
            .Where(x => x.Id == lessonId)
            .Include(l => l.Sections);
    }
}