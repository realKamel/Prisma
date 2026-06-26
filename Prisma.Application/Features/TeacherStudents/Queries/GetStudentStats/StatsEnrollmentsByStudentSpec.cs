using Ardalis.Specification;
using Prisma.Domain.Entities.EnrollmentAggregate;

namespace Prisma.Application.Features.TeacherStudents.Queries.GetStudentStats;

public class StatsEnrollmentsByStudentSpec : Specification<Enrollment>
{
    public StatsEnrollmentsByStudentSpec(Guid studentId)
    {
        Query.Where(e => e.StudentId == studentId);
    }
}
