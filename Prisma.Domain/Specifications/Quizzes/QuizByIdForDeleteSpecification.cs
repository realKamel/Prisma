using Ardalis.Specification;
using Prisma.Domain.Entities.QuizAggregate;

namespace Prisma.Domain.Specifications.Quizzes;

public class QuizByIdForDeleteSpecification : Specification<Quiz>
{
    public QuizByIdForDeleteSpecification(int quizId)
    {
        Query
            .Where(x => x.Id == quizId)
            .Include(x => x.Attempts)
            .Include(x => x.Questions);
    }
}
