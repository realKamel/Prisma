using Ardalis.Specification;
using Prisma.Domain.Entities.LessonAggregate;

namespace Prisma.Domain.Specifications.AcademicYearsSpecs;

public class FilteredAcademicYear : Specification<AcademicYear>
{
    public FilteredAcademicYear(IList<Guid> idsList)
    {
        Query.Where(x => idsList.Contains(x.PublicId));
    }
}
