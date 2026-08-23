using System.Linq.Expressions;
using Ardalis.Specification;
using Prisma.Domain.Entities.UserAggregate;

namespace Prisma.Domain.Specifications.Users;

public class UserWithProjectionSpec<TResult> : Specification<User, TResult>
{
    public UserWithProjectionSpec(Guid id, Expression<Func<User, TResult>> projection)
    {
        Query
            .Where(u => u.Id == id)
            .AsNoTracking()
            .Select(projection);
    }
}