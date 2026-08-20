using System.Linq.Expressions;
using Ardalis.Specification;
using Prisma.Domain.Entities.UserAggregate;

namespace Prisma.Domain.Specifications.AuditLogs;

public class AuditLogWithProjectionSpec<TResult> : Specification<AuditLog, TResult>
{
    public AuditLogWithProjectionSpec(string email, int take, Expression<Func<AuditLog, TResult>> projection)
    {
        Query
            .Where(l => l.UserEmail == email)
            .OrderByDescending(l => l.CreatedAt)
            .Take(take)
            .AsNoTracking()
            .Select(projection);
    }
}