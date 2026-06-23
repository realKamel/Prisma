using Ardalis.Specification;
using Prisma.Domain.Entities.UserAggregate;

namespace Prisma.Domain.Specifications.Students;

public class ActiveStudentSpecification : Specification<Student>
{
    public ActiveStudentSpecification()
    {
        Query.Where(s => s.IsOnline).AsNoTracking();
    }
}