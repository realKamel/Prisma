using Ardalis.Specification;
using Prisma.Domain.Entities.UserAggregate;

namespace Prisma.Domain.Specifications.Admin;

public class ActivityLogsFilterSpec : Specification<AuditLog>
{
    public ActivityLogsFilterSpec(int skip, int take)
    {
        Query
            .OrderByDescending(x => x.CreatedAt)
            .Skip(skip)
            .Take(take);
    }
}