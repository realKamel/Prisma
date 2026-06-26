using Ardalis.Specification;
using Prisma.Domain.Entities.LessonAggregate;

namespace Prisma.Domain.Specifications.Assistants;

public class AssistantLessonsSpec : Specification<Lesson>
{
    public AssistantLessonsSpec()
    {
        Query
            .Include(l => l.Enrollments)
            .Include(l => l.Sections)
            .OrderByDescending(l => l.Id)
            .AsSplitQuery()
            .AsNoTracking();
    }
}