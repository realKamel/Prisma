using System.Linq.Expressions;
using Ardalis.Specification;
using Prisma.Domain.Entities.UserAggregate;

namespace Prisma.Domain.Specifications.Assistants;

public class AssistantWithProjectionSpec<TResult> : Specification<Assistant, TResult>
{
    public AssistantWithProjectionSpec(Guid? teacherId, Expression<Func<Assistant, TResult>> projection)
    {
        Query
            .Where(a => a.TeacherId == teacherId)
            .AsNoTracking()
            .Select(projection);
    }
}