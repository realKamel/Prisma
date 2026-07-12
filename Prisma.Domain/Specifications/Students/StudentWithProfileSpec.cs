using Ardalis.Specification;
using Prisma.Domain.Entities.UserAggregate;

namespace Prisma.Domain.Specifications.Students;

public class StudentWithProfileSpec : Specification<Student>
{
    public StudentWithProfileSpec(Guid studentId)
    {
        Query.Where(s => s.Id == studentId)
            .Include(s => s.AcademicYear);
    }
}