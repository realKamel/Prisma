using Ardalis.Specification;
using Prisma.Domain.Entities.QuizAggregate;

namespace Prisma.Domain.Specifications.Quizzes;

public class QuizQuestionsByQuizIdSpecification
    : Specification<QuizQuestion>
{
    public QuizQuestionsByQuizIdSpecification(int quizId)
    {
        Query.Where(qq => qq.QuizId == quizId);
    }
}