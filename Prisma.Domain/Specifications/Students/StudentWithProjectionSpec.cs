using System.Linq.Expressions;
using Ardalis.Specification;
using Prisma.Domain.Entities.UserAggregate;

namespace Prisma.Domain.Specifications.Students;

public class StudentWithProjectionSpec<TResult> : Specification<Student, TResult>
{
    public StudentWithProjectionSpec(Guid id, Expression<Func<Student, TResult>> projection)
    {
        Query
            .Where(s => s.Id == id)
            .AsNoTracking()
            .Select(projection);
    }
}