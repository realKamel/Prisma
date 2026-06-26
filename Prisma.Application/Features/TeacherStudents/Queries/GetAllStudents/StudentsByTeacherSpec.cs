using Ardalis.Specification;
using Prisma.Domain.Entities.UserAggregate;

namespace Prisma.Application.Features.TeacherStudents.Queries.GetAllStudents;

public class StudentsByTeacherSpec : Specification<Student>
{
    public StudentsByTeacherSpec(Guid teacherId)
    {
        Query.Where(s => s.TeacherId == teacherId)
             .Include(s => s.Enrollments)
             .Include(s => s.QuizAttempts)
             .Include(s => s.AcademicYear);
    }
}
