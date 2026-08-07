using Ardalis.Specification;
using Prisma.Application.Features.Quizzes.Dtos;
using Prisma.Domain.Entities.EnrollmentAggregate;
using Prisma.Domain.Enums;

namespace Prisma.Application.Features.Quizzes.Specifications;

public class EnrolledStudentsByLessonSpecification
    : Specification<Enrollment, StudentListProjection>
{
    public EnrolledStudentsByLessonSpecification(int lessonId)
    {
        Query
            .Where(e =>
                e.LessonId == lessonId &&
                e.Status == EnrollmentStatus.Active)

            .Select(e => new StudentListProjection
            {
                Id = e.Student!.Id,
                FirstName = e.Student.FirstName,
                LastName = e.Student.LastName
            });
    }
}