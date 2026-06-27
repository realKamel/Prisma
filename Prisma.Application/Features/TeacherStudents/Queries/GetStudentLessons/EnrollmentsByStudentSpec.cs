using Ardalis.Specification;
using Prisma.Domain.Entities.EnrollmentAggregate;

namespace Prisma.Application.Features.TeacherStudents.Queries.GetStudentLessons;

public class EnrollmentsByStudentSpec : Specification<Enrollment>
{
    public EnrollmentsByStudentSpec(Guid studentId)
    {
        Query.Where(e => e.StudentId == studentId)
             .Include(e => e.Lesson)
             .Include(e => e.Student)
             .ThenInclude(s => s!.SectionProgresses)
             .ThenInclude(sp => sp.Section);
    }
}
