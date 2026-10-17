using Ardalis.Specification;
using Prisma.Domain.Entities.LessonAggregate;

namespace Prisma.Domain.Specifications.Lessons;
public class AssignmentWithEnrollmentSpec : Specification<Assignment>
{
    public AssignmentWithEnrollmentSpec(Guid lessonId)
        : base()
    {
        Query.Where(a => a.Lesson.PublicId == lessonId)
            .Include(a => a.Submissions)
            .Include(a => a.Lesson)
            .ThenInclude(l => l.Enrollments);
    }
}