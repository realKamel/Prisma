using Ardalis.Specification;
using Prisma.Domain.Entities.QuizAggregate;

namespace Prisma.Domain.Specifications.Quizzes;

public class QuizForFinalizationSpecification : Specification<Quiz>
{
    public QuizForFinalizationSpecification(int quizId)
    {
        Query
            .Where(q => q.Id == quizId)
            .Include(q => q.Questions)
                .ThenInclude(ql => ql.Question)
                .ThenInclude(question => (question as MCQQuestion)!.Choices);
    }
}
