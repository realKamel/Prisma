using Ardalis.Specification;
using Prisma.Domain.Entities.QuizAggregate;
using Prisma.Domain.Enums;

namespace Prisma.Domain.Specifications.Quizzes;

public class ExpiredInProgressAttemptsSpecification : Specification<QuizAttempt>
{
    public ExpiredInProgressAttemptsSpecification()
    {
        var now = DateTimeOffset.UtcNow;
        Query
            .Include(a => a.Answers)
            .Include(a => a.Quiz)
            .Where(a => a.Status == QuizAttemptStatus.InProgress
                     && a.StartedAt + a.Quiz.TimeInMinutes + TimeSpan.FromSeconds(10) < now);
    }
}