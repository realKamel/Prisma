using Ardalis.Specification;
using Prisma.Domain.Entities.LessonAggregate;

namespace Prisma.Domain.Specifications.Lessons;

public class LessonStatusSpecification : Specification<Lesson>
{
    public LessonStatusSpecification(int lessonId)
        : base()
    {
        Query.Where(lesson => lesson.Id == lessonId)
            .Include(l => l.Enrollments)
            .Include(l => l.Prerequisite)
            .ThenInclude(p => p.Enrollments);
    }
}