using Ardalis.Specification;
using Prisma.Domain.Entities.LessonAggregate;

namespace Prisma.Domain.Specifications.Lessons;
public class AssignmentWithEnrollmentSpec : Specification<Assignment>
{
    public AssignmentWithEnrollmentSpec(int lessonId)
        : base()
    {
        Query.Where(a => a.LessonId == lessonId)
            .Include(a => a.Submissions)
            .Include(a => a.Lesson)
            .ThenInclude(l => l.Enrollments);
    }
}