using Ardalis.Specification;
using Prisma.Domain.Entities.UserAggregate;

namespace Prisma.Domain.Specifications.Users;

public class UserByEmailSpecification : Specification<User>
{
    public UserByEmailSpecification(string email)
    {
        Query.Where(u => u.Email == email);
    }
}
