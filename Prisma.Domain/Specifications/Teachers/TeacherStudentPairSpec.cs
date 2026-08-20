using Ardalis.Specification;
using Prisma.Domain.Entities.UserAggregate;

namespace Prisma.Domain.Specifications.Teachers;

public sealed class TeacherStudentPairSpec : Specification<TeacherStudent>
{
    public TeacherStudentPairSpec(Guid teacherId, Guid studentId)
    {
        Query.Where(ts => ts.TeacherId == teacherId && ts.StudentId == studentId);
    }
}
