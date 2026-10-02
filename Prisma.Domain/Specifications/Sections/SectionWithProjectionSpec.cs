using System.Linq.Expressions;
using Ardalis.Specification;
using Prisma.Domain.Entities.LessonAggregate;

namespace Prisma.Domain.Specifications.Sections;

public class SectionWithProjectionSpec<TResult> : Specification<Section, TResult>
{
    public SectionWithProjectionSpec(int id, Expression<Func<Section, TResult>> projection)
    {
        Query
            .Where(l => l.Id == id)
            .AsNoTracking()
            .Select(projection);
    }
}