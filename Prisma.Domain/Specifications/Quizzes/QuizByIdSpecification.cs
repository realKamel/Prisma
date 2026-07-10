using Ardalis.Specification;
using Prisma.Domain.Entities.QuizAggregate;

namespace Prisma.Domain.Specifications.Quizzes;

public class QuizByIdSpecification : Specification<Quiz>
{
    public QuizByIdSpecification(int quizId)
    {
        Query.Where(q => q.Id == quizId);
    }
}