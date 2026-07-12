using Ardalis.Specification;
using Prisma.Domain.Entities.LessonAggregate;

namespace Prisma.Domain.Specifications.Lessons;

public class LessonWithDetailsSpecification : Specification<Lesson>
{
    public LessonWithDetailsSpecification(int lessonId)
    {
        Query.Where(lesson => lesson.Id == lessonId)
            .Include(l => l.Sections)
            .Include(l => l.Enrollments)
            .Include(l => l.Prerequisite)
            .Include(l => l.Quiz)
            .Include(l => l.LessonMaterials)
            .AsNoTracking();
    }
}