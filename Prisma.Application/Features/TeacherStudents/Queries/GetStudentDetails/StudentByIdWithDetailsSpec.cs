using Ardalis.Specification;
using Prisma.Domain.Entities.UserAggregate;

namespace Prisma.Application.Features.TeacherStudents.Queries.GetStudentDetails;

public class StudentByIdWithDetailsSpec : Specification<Student>
{
    public StudentByIdWithDetailsSpec(Guid studentId)
    {
        Query.Where(s => s.Id == studentId)
             .Include(s => s.Enrollments)
             .Include(s => s.QuizAttempts)
             .Include(s => s.AcademicYear);
    }
}
