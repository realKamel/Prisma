using Ardalis.Specification;
using Prisma.Domain.Entities.LessonAggregate;

namespace Prisma.Domain.Specifications.Assignments;

public class AssignmentsWithLessonSpecification : Specification<Assignment>
{
    public AssignmentsWithLessonSpecification(Guid teacherId)
    {
        Query.Include(a => a.Lesson)
            .Where(a => a.Lesson.TeacherId == teacherId);
    }
}