using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Prisma.Application.Abstractions.Services;
using Prisma.Domain.Entities.UserAggregate;

namespace Prisma.Infrastructure.Persistence.Interceptors;

internal class AuditLogInterceptor(ICurrentUserService currentUser) : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        AddAuditLogs(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        AddAuditLogs(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void AddAuditLogs(DbContext? context)
    {
        if (context is null) return;

        var entries = BuildAuditLogs(context);
        if (entries.Count > 0)
            context.Set<AuditLog>().AddRange(entries);
    }

    private List<AuditLog> BuildAuditLogs(DbContext context)
    {
        var now = DateTimeOffset.UtcNow;
        var userEmail = currentUser.Email ?? "System";
        var auditEntries = new List<AuditLog>();

        foreach (var entry in context.ChangeTracker.Entries())
        {
            if (entry.Entity is AuditLog
                || entry.State is EntityState.Detached or EntityState.Unchanged)
                continue;

            var action = entry.State switch
            {
                EntityState.Added => "CREATE",
                EntityState.Modified => "UPDATE",
                EntityState.Deleted => "DELETE",
                _ => entry.State.ToString().ToUpper()
            };

            var auditLog = new AuditLog
            {
                UserEmail = userEmail,
                TableName = entry.Metadata.GetTableName() ?? "",
                Action = action,
                CreatedAt = now
            };

            var oldValues = new Dictionary<string, object?>();
            var newValues = new Dictionary<string, object?>();

            foreach (var property in entry.Properties)
            {
                var name = property.Metadata.Name;

                if (property.Metadata.IsPrimaryKey())
                {
                    auditLog.EntityId = property.CurrentValue?.ToString() ?? "";
                    continue;
                }

                switch (entry.State)
                {
                    case EntityState.Added:
                        newValues[name] = property.CurrentValue;
                        break;
                    case EntityState.Deleted:
                        oldValues[name] = property.OriginalValue;
                        break;
                    case EntityState.Modified when property.IsModified:
                        oldValues[name] = property.OriginalValue;
                        newValues[name] = property.CurrentValue;
                        break;
                }
            }

            auditLog.OldValues = oldValues.Count == 0 ? null : JsonSerializer.Serialize(oldValues);
            auditLog.NewValues = newValues.Count == 0 ? null : JsonSerializer.Serialize(newValues);
            auditEntries.Add(auditLog);
        }

        return auditEntries;
    }
}