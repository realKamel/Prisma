
using Prisma.Domain.Enums;

namespace Prisma.Application.Abstractions.Services;

public interface IAttemptSecurityCounter
{
    /// <summary>
    /// Increments the security counter for the specified quiz attempt.
    /// </summary>
    /// <param name="attemptId"></param>
    /// <param name="studentId"></param>
    /// <param name="eventType"></param>
    /// <param name="ct"></param>
    /// <returns>true if a counter was incremented; false if the attempt is missing,
    /// not owned by the student, or no longer InProgress.</returns>
    Task<bool> IncrementAsync(int attemptId, Guid studentId, SecurityEventType eventType, CancellationToken ct);
}