using Ardalis.Specification;
using Prisma.Domain.Entities.EnrollmentAggregate;

namespace Prisma.Application.Features.TeacherStudents;

public class EnrollmentByStudentAndLessonSpec : Specification<Enrollment>
{
    public EnrollmentByStudentAndLessonSpec(Guid studentId, int lessonId)
    {
        Query.Where(e => e.StudentId == studentId && e.LessonId == lessonId);
    }
}
