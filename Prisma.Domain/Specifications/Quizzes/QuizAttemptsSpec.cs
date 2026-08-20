using Ardalis.Specification;
using Prisma.Domain.Entities.QuizAggregate;
using Prisma.Domain.Enums;

namespace Prisma.Domain.Specifications.Quizzes;

public sealed class QuizAttemptsSpec : Specification<QuizAttempt>
{
    public QuizAttemptsSpec(
        Guid teacherId,
        DateTimeOffset from,
        DateTimeOffset? to = null,
        QuizAttemptStatus? status = null)
    {
        Query
            .Where(a => a.SubmittedAt >= from
                     && (to == null || a.SubmittedAt < to)
                     && (status == null || a.Status == status)
                     && a.Quiz!.Lesson!.TeacherId == teacherId)
            .AsNoTracking();
    }
}