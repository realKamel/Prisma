using Ardalis.Specification;
using Prisma.Domain.Entities.UserAggregate;

namespace Prisma.Domain.Specifications.Students;

public class ActiveStudentSpecification : Specification<Student>
{
    public ActiveStudentSpecification(Guid? teacherId = null)
    {
        Query
            .Where(s => s.IsOnline
                && (teacherId == null || s.TeacherStudents.Any(ts => ts.TeacherId == teacherId && !ts.IsKicked)))
            .AsNoTracking();
    }
}