using Ardalis.Specification;
using Prisma.Domain.Entities.LessonAggregate;

namespace Prisma.Domain.Specifications.Lessons;

public class LessonWithEnrollmentSpec : Specification<Lesson>
{
    public LessonWithEnrollmentSpec(int lessonId)
        : base()
    {
        Query.Where(lesson => lesson.Id == lessonId)
            .Include(l => l.Enrollments);
    }
}