using Microsoft.EntityFrameworkCore;
using Prisma.Application.Abstractions.Services;
using Prisma.Application.Features.Quizzes.Common;
using Prisma.Domain.Entities.QuizAggregate;
using Prisma.Domain.Enums;
using Prisma.Infrastructure.Persistence;

namespace Prisma.Infrastructure.Services.AttemptSecurityCounter;

internal sealed class AttemptSecurityCounter(AppDbContext db) : IAttemptSecurityCounter
{
    public async Task<bool> IncrementAsync(
        int attemptId, Guid studentId, SecurityEventType eventType, CancellationToken ct)
    {
        var now = DateTimeOffset.UtcNow;
        var acceptEventsAfter = now - TimeSpan.FromSeconds(QuizConstants.GracePeriodSeconds);

        // Status check lives in the WHERE clause, so check + increment is a single atomic statement
        var query = db.Set<QuizAttempt>().Where(a =>
            a.Id == attemptId &&
            a.StudentId == studentId &&
            a.Status == QuizAttemptStatus.InProgress && a.Deadline >= acceptEventsAfter);

        var rows = eventType switch
        {
            SecurityEventType.TabSwitch => await query.ExecuteUpdateAsync(s => s
                .SetProperty(a => a.TabSwitchCount, a => a.TabSwitchCount + 1)
                .SetProperty(a => a.UpdatedAt, now)
                .SetProperty(a => a.UpdatedBy, studentId), ct),

            SecurityEventType.CopyPasteAttempt => await query.ExecuteUpdateAsync(s => s
                .SetProperty(a => a.CopyPasteAttemptCount, a => a.CopyPasteAttemptCount + 1)
                .SetProperty(a => a.UpdatedAt, now)
                .SetProperty(a => a.UpdatedBy, studentId), ct),

            _ => 0
        };

        return rows > 0;
    }
}