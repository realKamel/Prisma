using Ardalis.Specification;
using Prisma.Domain.Entities.LessonAggregate;

namespace Prisma.Domain.Specifications.Lessons;

public class UpdateLessonDetailsSpecification : Specification<Lesson>
{
    public UpdateLessonDetailsSpecification(int lessonId)
    {
        Query.Where(lesson => lesson.Id == lessonId && !lesson.IsDeleted)
            .Include(l => l.Sections)
            .Include(l => l.Assignment)
            .Include(l => l.AcademicYears);
    }
}