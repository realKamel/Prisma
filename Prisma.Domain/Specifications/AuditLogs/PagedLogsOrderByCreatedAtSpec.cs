using System.Linq.Expressions;
using Ardalis.Specification;
using Prisma.Domain.Entities.UserAggregate;

namespace Prisma.Domain.Specifications.AuditLogs;

public class PagedLogsOrderByCreatedAtSpec<TResult> : Specification<AuditLog, TResult>
{
    public PagedLogsOrderByCreatedAtSpec(Expression<Func<AuditLog, bool>> expression, Expression<Func<AuditLog, TResult>> selector, int pageNumber, int pageSize)
    {
        Query
        .Where(expression)
        .OrderByDescending(x => x.CreatedAt)
        .Skip((pageNumber - 1) * pageSize)
        .Take(pageSize)
        .AsNoTracking()
        .Select(selector);
    }
}