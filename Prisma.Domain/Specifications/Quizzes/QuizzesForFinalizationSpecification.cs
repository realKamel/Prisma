using Ardalis.Specification;
using Prisma.Domain.Entities.QuizAggregate;

namespace Prisma.Domain.Specifications.Quizzes;

public class QuizzesForFinalizationSpecification : Specification<Quiz>
{
    public QuizzesForFinalizationSpecification(IReadOnlyCollection<int> quizIds)
    {
        Query
            .Where(q => quizIds.Contains(q.Id))
            .Include(q => q.Questions)
                .ThenInclude(ql => ql.Question)
                .ThenInclude(question => (question as MCQQuestion)!.Choices)
            .AsSplitQuery();
    }
}
