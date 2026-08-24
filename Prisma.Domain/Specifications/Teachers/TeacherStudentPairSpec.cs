using Ardalis.Specification;

namespace Prisma.Domain.Specifications.Teachers;

public sealed class TeacherStudentPairSpec : Specification<Prisma.Domain.Entities.UserAggregate.TeacherStudent>
{
    public TeacherStudentPairSpec(Guid teacherId, Guid studentId)
    {
        Query.Where(ts => ts.TeacherId == teacherId && ts.StudentId == studentId);
    }
}
