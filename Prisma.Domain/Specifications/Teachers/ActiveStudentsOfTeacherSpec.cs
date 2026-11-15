using Ardalis.Specification;

namespace Prisma.Domain.Specifications.Teachers;

/// <summary>A teacher's students: TeacherStudents rows for the teacher, minus kicked students.</summary>
public sealed class ActiveStudentsOfTeacherSpec : Specification<Prisma.Domain.Entities.UserAggregate.TeacherStudent>
{
    public ActiveStudentsOfTeacherSpec(Guid teacherId)
    {
        Query.Where(ts => ts.TeacherId == teacherId && !ts.IsKicked);
    }
}
