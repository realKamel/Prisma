using System.Linq.Expressions;
using Ardalis.Specification;
using Prisma.Domain.Entities.UserAggregate;

namespace Prisma.Domain.Specifications.Teachers;

public class StudentsByTeacherSpec<TResult> : Specification<Student, TResult>
{
    public StudentsByTeacherSpec(Guid teacherId, Expression<Func<Student, TResult>> projection)
    {
        Query
            .Where(s => s.TeacherStudents.Any(ts => ts.TeacherId == teacherId))
            .AsNoTracking()
            .Select(projection);
    }
}
