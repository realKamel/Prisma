using Ardalis.Specification;
using Prisma.Domain.Entities.EnrollmentAggregate;
using Prisma.Domain.Entities.UserAggregate;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

public sealed class EnrollmentWithStudentAndLessonSpec : Specification<Enrollment>
{
    public EnrollmentWithStudentAndLessonSpec(int enrollmentId)
    {
        Query.Where(e => e.Id == enrollmentId)
             .Include(e => e.Student)
                .ThenInclude(s => s.AcademicYear)
             .Include(e => e.Lesson);
    }
}

public sealed class StudentWithAcademicYearSpec : Specification<Student>
{
    public StudentWithAcademicYearSpec(Guid studentId)
    {
        Query.Where(s => s.Id == studentId)
             .Include(s => s.AcademicYear);
    }
}