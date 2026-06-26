using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Prisma.Application.Abstractions.Services;
using Prisma.Domain.Entities.UserAggregate;

namespace Prisma.Infrastructure.Persistence.Interceptors;

internal class AuditLogInterceptor(ICurrentUserService currentUser) : SaveChangesInterceptor
{
    public override int SavedChanges(SaveChangesCompletedEventData eventData, int result)
    {

        var auditEntries = OnBeforeSaveChanges(eventData.Context);

        if (auditEntries?.Count > 0)
        {
            eventData.Context?.Set<AuditLog>().AddRange(auditEntries);
        }
        return base.SavedChanges(eventData, result);
    }
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        var auditEntries = OnBeforeSaveChanges(eventData.Context);

        if (auditEntries?.Count > 0)
        {
            eventData.Context?.Set<AuditLog>().AddRange(auditEntries);
        }
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private List<AuditLog> OnBeforeSaveChanges(DbContext? context)
    {
        if (context is null)
        {
            return [];
        }

        var now = DateTimeOffset.UtcNow;

        var userId = currentUser.UserId ?? Guid.Empty; // means system
        var userEmail = currentUser.Email ?? "";
        var auditEntries = new List<AuditLog>();

        foreach (var entry in context.ChangeTracker.Entries())
        {
            if (entry.Entity is AuditLog || entry.State == EntityState.Detached || entry.State == EntityState.Unchanged)
                continue;

            var auditLog = new AuditLog
            {
                UserEmail = userEmail,
                TableName = entry.Metadata.GetTableName() ?? "",
                Action = entry.State.ToString().ToUpper() // CREATE, UPDATE, DELETE
            };
            var oldValues = new Dictionary<string, object>();
            var newValues = new Dictionary<string, object>();

            foreach (var property in entry.Properties)
            {
                string propertyName = property.Metadata.Name;
                if (property.Metadata.IsPrimaryKey())
                {
                    auditLog.EntityId = property.CurrentValue?.ToString() ?? "";
                    continue;
                }

                switch (entry.State)
                {
                    case EntityState.Added:
                        newValues[propertyName] = property.CurrentValue ?? "";
                        break;
                    case EntityState.Deleted:
                        oldValues[propertyName] = property.OriginalValue ?? "";
                        break;
                    case EntityState.Modified:
                        if (property.IsModified)
                        {
                            oldValues[propertyName] = property.OriginalValue ?? "";
                            newValues[propertyName] = property.CurrentValue ?? "";
                        }
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
