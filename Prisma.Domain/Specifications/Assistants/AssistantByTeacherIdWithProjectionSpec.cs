using System.Linq.Expressions;
using Ardalis.Specification;
using Prisma.Domain.Entities.UserAggregate;

namespace Prisma.Domain.Specifications.Assistants;

public sealed class AssistantByTeacherIdWithProjectionSpec<TResult>
    : Specification<Assistant, TResult>
{
    public AssistantByTeacherIdWithProjectionSpec(
        Guid teacherId,
        Expression<Func<Assistant, TResult>> projection,
        bool asNoTracking = false
    )
    {
        Query.Where(a => a.Id == teacherId);

        if (asNoTracking)
        {
            Query.AsNoTracking();
        }

        Query.Select(projection);
    }
}
