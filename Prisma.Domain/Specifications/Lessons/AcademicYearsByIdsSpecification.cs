using Ardalis.Specification;
using Prisma.Domain.Entities.LessonAggregate;

namespace Prisma.Domain.Specifications.Lessons;
public class AcademicYearsByIdsSpecification : Specification<AcademicYear>
{
    public AcademicYearsByIdsSpecification(IEnumerable<int> ids)
    {
        Query.Where(ay => ids.Contains(ay.Id));
    }
}