using Ardalis.Specification;
using Prisma.Domain.Entities.LessonAggregate;

namespace Prisma.Domain.Specifications.AcademicYearsSpecs;

public sealed class AcademicYearByIdSpec : Specification<AcademicYear>
{
    public AcademicYearByIdSpec(int id)
    {
        Query.Where(x => x.Id == id).Include(x => x.Teachers);
    }
}
