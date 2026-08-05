using Ardalis.Specification;
using Prisma.Domain.Entities.LessonAggregate;
using Prisma.Domain.Enums;

namespace Prisma.Domain.Specifications.Lessons;

public class LessonPrerequisiteCompletionSpecification : Specification<Lesson, bool>
{
    public LessonPrerequisiteCompletionSpecification(int prerequisiteLessonId, Guid studentId)
    {
        Query
            .Where(lesson => lesson.Id == prerequisiteLessonId).AsNoTracking()
            .Select(lesson => lesson.Enrollments.Any(e =>
                e.StudentId == studentId &&
                e.Status == EnrollmentStatus.Active &&
                e.IsCompleted));
    }
}