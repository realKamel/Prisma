using Ardalis.Specification;
using Prisma.Domain.Entities.EnrollmentAggregate;

namespace Prisma.Application.Features.TeacherStudents.Queries.GetStudentActivities;

public class EnrollmentsByStudentForActivitySpec : Specification<Enrollment>
{
    public EnrollmentsByStudentForActivitySpec(Guid studentId)
    {
        Query.Where(e => e.StudentId == studentId)
             .Include(e => e.Lesson)
             .OrderByDescending(e => e.CreatedAt);
    }
}
