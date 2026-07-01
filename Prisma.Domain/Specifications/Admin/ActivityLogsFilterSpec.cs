using System;
using Ardalis.Specification;
using Prisma.Domain.Entities.UserAggregate;

namespace Prisma.Domain.Specifications.AuditLogs;

public sealed class ActivityLogsFilterSpec : Specification<AuditLog>
{
    public ActivityLogsFilterSpec(int take)
    {
        Query
            .OrderByDescending(l => l.CreatedAt)
            .Take(take)
            .AsNoTracking();
    }
}