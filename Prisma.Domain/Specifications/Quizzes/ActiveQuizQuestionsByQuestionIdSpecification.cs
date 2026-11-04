using Ardalis.Specification;
using Prisma.Domain.Entities.QuizAggregate;

namespace Prisma.Domain.Specifications.Quizzes;

public class ActiveQuizQuestionsByQuestionIdSpecification
    : Specification<QuizQuestion>
{
    public ActiveQuizQuestionsByQuestionIdSpecification(int questionId)
    {
        Query.Where(qq =>
            qq.QuestionId == questionId &&
            !qq.IsDeleted);
    }
}