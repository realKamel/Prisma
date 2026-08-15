using System.Linq.Expressions;
using Ardalis.Specification;
using Prisma.Domain.Entities.LessonAggregate;

namespace Prisma.Domain.Specifications.Lessons;

public class LessonWithProjectionSpec<TResult> : Specification<Lesson, TResult>
{
    public LessonWithProjectionSpec(int id, Expression<Func<Lesson, TResult>> projection)
    {
        Query
            .Where(l => l.Id == id)
            .AsNoTracking()
            .Select(projection);
    }
}