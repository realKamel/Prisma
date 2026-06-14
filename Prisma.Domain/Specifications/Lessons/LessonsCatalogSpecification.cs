using Ardalis.Specification;
using Prisma.Domain.Entities.LessonAggregate;

namespace Prisma.Domain.Specifications.Lessons;

public class LessonsCatalogSpecification : Specification<Lesson>
{
    public LessonsCatalogSpecification()
    {
        Query.Include(x => x.Enrollments)
            .Include(x => x.Sections)
            .ThenInclude(s => s.Progresses);

        // AddInclude(x => x.Enrollments);
        // AddInclude(x => x.Sections);
        // AddInclude($"{nameof(Lesson.Sections)}.{nameof(Section.Progresses)}");
    }
}