using Ardalis.Specification;
using Prisma.Domain.Entities.QuizAggregate;
using Prisma.Domain.Enums;

namespace Prisma.Domain.Specifications.Quizzes;

public class ExpiredInProgressAttemptsSpecification : Specification<QuizAttempt>
{
    // Cursor pagination (Id > afterId) بدل Skip: أسرع، ومفيش خطر الـ loop اللانهائي لو attempt اتخطّى
    public ExpiredInProgressAttemptsSpecification(int afterId, int batchSize, TimeSpan gracePeriod)
    {
        var cutoff = DateTimeOffset.UtcNow - gracePeriod;

        Query
            .Where(a => a.Id > afterId
                     && a.Status == QuizAttemptStatus.InProgress
                     && a.Deadline < cutoff)
            .OrderBy(a => a.Id)
            .Take(batchSize)
            .Include(a => a.Answers);
    }
}