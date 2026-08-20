using System.Linq.Expressions;
using Ardalis.Specification;

namespace Prisma.Domain.Specifications.Admin;

public class AdminWithProjectionSpec<TResult> : Specification<Prisma.Domain.Entities.UserAggregate.Admin, TResult>
{
    public AdminWithProjectionSpec(Guid id, Expression<Func<Prisma.Domain.Entities.UserAggregate.Admin, TResult>> projection)
    {
        Query
            .Where(a => a.Id == id)
            .AsNoTracking()
            .Select(projection);
    }
}