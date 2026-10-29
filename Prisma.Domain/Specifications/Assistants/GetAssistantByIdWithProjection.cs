using System.Linq.Expressions;
using Ardalis.Specification;
using Prisma.Domain.Entities.UserAggregate;

namespace Prisma.Domain.Specifications.Assistants;

public class GetAssistantByIdWithProjection<TResult> : Specification<Assistant, TResult>
{
    public GetAssistantByIdWithProjection(
        Guid userId,
        Expression<Func<Assistant, TResult>> projection
    )
    {
        Query.Where(a => a.Id == userId).AsNoTracking().Select(projection);
    }
}
