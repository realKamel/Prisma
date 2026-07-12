using Ardalis.Specification;
using Prisma.Domain.Entities.QuizAggregate;
using Prisma.Domain.Enums;

namespace Prisma.Domain.Specifications.Quizzes;

public sealed class QuizAttemptsSpec : Specification<QuizAttempt>
{
    public QuizAttemptsSpec(
        DateTimeOffset from,
        DateTimeOffset? to = null,
        QuizAttemptStatus? status = null)
    {
        Query
            .Where(a => a.SubmittedAt >= from
                     && (to == null     || a.SubmittedAt < to)
                     && (status == null || a.Status == status))
            .Include(a => a.Quiz)
            .AsNoTracking();
    }
}