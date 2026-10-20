
using Ardalis.Specification;
using Prisma.Domain.Entities.QuizAggregate;

namespace Prisma.Domain.Specifications.Quizzes;

// The choice must belong to this question
public sealed class ChoiceForQuestionSpecification : Specification<Choice>
{
    public ChoiceForQuestionSpecification(int questionId, int choiceId)
    {
        Query.Where(c => c.Id == choiceId && c.QuestionId == questionId);
    }
}