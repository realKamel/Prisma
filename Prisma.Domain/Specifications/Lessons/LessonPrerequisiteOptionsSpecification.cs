using Ardalis.Specification;
using Prisma.Domain.Entities.LessonAggregate;

namespace Prisma.Domain.Specifications.Lessons;

public class LessonPrerequisiteOptionsSpecification : Specification<Lesson>
{
    public LessonPrerequisiteOptionsSpecification(int excludeLessonId)
    {
        Query.Where(l => l.Id != excludeLessonId);
    }
}