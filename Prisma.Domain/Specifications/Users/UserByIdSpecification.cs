using Ardalis.Specification;
using Prisma.Domain.Entities.UserAggregate;

namespace Prisma.Domain.Specifications.Users;

public class UserByIdSpecification : Specification<User>
{
    public UserByIdSpecification(Guid id)
    {
        Query.Where(u => u.Id == id);
    }
}