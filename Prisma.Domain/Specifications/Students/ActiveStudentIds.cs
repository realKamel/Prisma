using Ardalis.Specification;
using Prisma.Domain.Entities.UserAggregate;

namespace Prisma.Domain.Specifications.Students;

public class ActiveStudentIds : Specification<Student, Guid>
{
    public ActiveStudentIds()
    {
        Query
            .AsNoTracking()
            .Where(x => !x.IsBlocked)
            .Select(x => x.Id);
    }
}