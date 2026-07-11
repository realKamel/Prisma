
using Ardalis.Specification;
using Prisma.Domain.Entities.UserAggregate;

namespace Prisma.Application.Features.TeacherStudents.Queries.GetAllStudents;

public class StudentsByTeacherSpec : Specification<Student>
{
    public StudentsByTeacherSpec()
    {
        Query
             .Include(s => s.Enrollments)
                 .ThenInclude(e => e.Lesson)
             .Include(s => s.QuizAttempts)
             .Include(s => s.AcademicYear);
    }
}