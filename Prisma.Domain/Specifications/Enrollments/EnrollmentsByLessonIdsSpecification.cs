using Ardalis.Specification;
using Prisma.Domain.Entities.EnrollmentAggregate;


namespace Prisma.Domain.Specifications.Enrollments;

public class EnrollmentsByLessonIdsSpecification : Specification<Enrollment>
{
    public EnrollmentsByLessonIdsSpecification(List<int> lessonIds)
    {
        Query
            .Where(e => lessonIds.Contains(e.LessonId!.Value))
            .Include(e => e.Student);
    }
}