using Ardalis.Specification;
using Prisma.Domain.Entities.QuizAggregate;


namespace Prisma.Domain.Specifications.Quizzes;

public class QuizWithAttemptsSpecification : Specification<Quiz>
{
    public QuizWithAttemptsSpecification(int quizId)
    {
        Query
            .Where(q => q.Id == quizId)
            .Include(q => q.Attempts)
                .ThenInclude(a => a.Student)
            .Include(q => q.Attempts)
                .ThenInclude(a => a.Answers);
    }
}
