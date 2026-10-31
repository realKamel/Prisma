using System.Linq.Expressions;
using Ardalis.Specification;
using Prisma.Domain.Entities.UserAggregate;

namespace Prisma.Domain.Specifications.Assistants;

public sealed class AssistantByIdWithProjectionSpec<TResult> : Specification<Assistant, TResult>
{
    public AssistantByIdWithProjectionSpec(
        Guid userId,
        Expression<Func<Assistant, TResult>> projection,
        bool asNoTracking = false
    )
    {
        Query.Where(a => a.Id == userId).Select(projection);

        if (asNoTracking)
        {
            Query.AsNoTracking();
        }

        Query.Select(projection);
    }
}
