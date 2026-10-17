
using Ardalis.Specification;
using Prisma.Domain.Entities.QuizAggregate;
namespace Prisma.Domain.Specifications.Quizzes;

public sealed class ChoicesByIdsSpecification : Specification<Choice>
{
    public ChoicesByIdsSpecification(IReadOnlyCollection<int> choiceIds)
    {
        Query.Where(c => choiceIds.Contains(c.Id));
    }
}