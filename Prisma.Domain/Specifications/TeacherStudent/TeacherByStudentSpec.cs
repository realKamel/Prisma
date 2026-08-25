using Ardalis.Specification;

namespace Prisma.Domain.Specifications.TeacherStudent;

public class TeacherByStudentSpec : Specification<Prisma.Domain.Entities.UserAggregate.TeacherStudent>
{
    public TeacherByStudentSpec(Guid studentId)
    {
        Query
            .Where(a => a.StudentId == studentId);
    }
}
