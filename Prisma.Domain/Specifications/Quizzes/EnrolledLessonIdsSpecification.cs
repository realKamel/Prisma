using Ardalis.Specification;
using Prisma.Domain.Entities.EnrollmentAggregate;
using Prisma.Domain.Entities.UserAggregate;
using Prisma.Domain.Enums;

namespace Prisma.Domain.Specifications.Quizzes;

public class EnrolledLessonIdsSpecification: Specification<Enrollment, int?>
{
    public EnrolledLessonIdsSpecification(Guid studentId)
    {
        Query.Where(e => e.StudentId == studentId && e.Status == EnrollmentStatus.Active)
            .Select(e => e.LessonId);
    }
}
