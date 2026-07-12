using Ardalis.Specification;
using Prisma.Domain.Entities.QuizAggregate;

namespace Prisma.Domain.Specifications.Quizzes;

public class AttemptWithQuizSpecification : Specification<QuizAttempt>
{
    public AttemptWithQuizSpecification(int attemptId)
    {
        Query
            .Where(a => a.Id == attemptId)
            .Include(a => a.Quiz);
    }
}

