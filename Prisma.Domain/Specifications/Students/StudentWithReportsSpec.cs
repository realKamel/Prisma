using Ardalis.Specification;
using Prisma.Domain.Entities.UserAggregate;

namespace Prisma.Domain.Specifications.Students;

public class StudentWithReportsSpec : Specification<Student>
{
    public StudentWithReportsSpec(Guid studentId)
    {
        Query
            .Where(s => s.Id == studentId)
            .Include(x => x.Reports);
    }
}