using Ardalis.Specification;
using Prisma.Domain.Entities.EnrollmentAggregate;
using Prisma.Domain.Enums;

namespace Prisma.Domain.Specifications.Quizzes;

public class EnrolledStudentsByLessonSpecification : Specification<Enrollment>
{
    public EnrolledStudentsByLessonSpecification(int lessonId)
    {
        Query
            .Where(e => e.LessonId == lessonId
                     && e.Status == EnrollmentStatus.Active)
            .Include(e => e.Student);
    }
}
