using System.Text;
using Ardalis.Specification;
using Prisma.Domain.Entities.EnrollmentAggregate;
using Prisma.Domain.Enums;

namespace Prisma.Domain.Specifications.Assignments;

public class EnrolledStudentsByLessonIdsSpecification : Specification<Enrollment>
{
    public EnrolledStudentsByLessonIdsSpecification(List<int> lessonIds)
    {
        Query
            .Where(e => lessonIds.Contains(e.LessonId!.Value)
                     && e.Status == EnrollmentStatus.Active)
            .Include(e => e.Student);
    }
}
