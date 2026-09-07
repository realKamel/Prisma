using System.Linq.Expressions;
using Ardalis.Specification;
using Prisma.Domain.Entities.UserAggregate;

namespace Prisma.Application.Features.TeacherStudents.Queries.GetStudentDetails;

public class StudentByIdWithDetailsSpec<TResult> : Specification<Student, TResult>
{
    public StudentByIdWithDetailsSpec(Guid studentId, Expression<Func<Student, TResult>> projection)
    {
        Query.Where(s => s.Id == studentId).AsNoTracking()
             .Select(projection);
    }
}
