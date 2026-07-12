using Ardalis.Specification;
using Prisma.Domain.Entities.QuizAggregate;

namespace Prisma.Domain.Specifications.Quizzes;

public class AttemptDetailForGradingSpecification: Specification<QuizAttempt>
{
    public AttemptDetailForGradingSpecification(int attemptId)
    {
        Query
            .Where(a => a.Id == attemptId)
            .Include(a => a.Student)
            .Include(a => a.Answers)
            .Include(a => a.Quiz)
                .ThenInclude(q => q.Questions)
                    .ThenInclude(ql => ql.Question)
                        .ThenInclude(q => (q as MCQQuestion)!.Choices);
    }
}
