using Ardalis.Specification;
using Prisma.Domain.Entities.UserAggregate;

namespace Prisma.Domain.Specifications.Users;

public class UsersByIdsSpecification : Specification<User>
{
    public UsersByIdsSpecification(List<Guid> ids)
    {
        Query.Where(u => ids.Contains(u.Id));
    }
}