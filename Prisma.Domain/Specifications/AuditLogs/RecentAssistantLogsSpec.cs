using Ardalis.Specification;
using Prisma.Domain.Entities.UserAggregate;

namespace Prisma.Domain.Specifications.AuditLogs;
public sealed class RecentAssistantLogsSpec : Specification<AuditLog>
{
    public RecentAssistantLogsSpec(string email, int take)
    {
        Query
            .Where(l => l.UserEmail == email)
            .OrderByDescending(l => l.CreatedAt)
            .Take(take)
            .AsNoTracking();
    }
}