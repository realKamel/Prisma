using Ardalis.Specification;
using Prisma.Domain.Entities.QuizAggregate;

namespace Prisma.Domain.Specifications.Quizzes;

public class AttemptByIdSpecification : Specification<QuizAttempt>
{
    public AttemptByIdSpecification(int attemptId)
    {
        Query
            .Where(a => a.Id == attemptId);
    }
}

