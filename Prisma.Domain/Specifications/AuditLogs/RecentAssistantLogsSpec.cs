using Ardalis.Specification;
using Prisma.Domain.Entities.UserAggregate;

namespace Prisma.Domain.Specifications.AuditLogs;

public sealed class RecentAssistantLogsSpec : Specification<AuditLog>
{
    public RecentAssistantLogsSpec(string email, int take)
    {
        Query
            .Where(l => l.UserEmail == email && (
                (l.Action == "insert" && l.TableName == "enrollments")           ||
                (l.Action == "delete" && l.TableName == "enrollments")           ||
                (l.Action == "insert" && l.TableName == "assignmentsubmissions") ||
                (l.Action == "update" && l.TableName == "assignmentsubmissions") ||
                (l.Action == "insert" && l.TableName == "quizattempts")          ||
                (l.Action == "update" && l.TableName == "quizattempts")          ||
                (l.Action == "select" && l.TableName == "students")              ||
                (l.Action == "insert" && l.TableName == "auditlogs")
            ))
            .OrderByDescending(l => l.CreatedAt)
            .Take(take)
            .AsNoTracking();
    }
}