using Ardalis.Specification;
using Prisma.Domain.Entities.QuizAggregate;

namespace Prisma.Domain.Specifications.Quizzes;

public class QuizStructureSpecification : Specification<Quiz>
{
    public QuizStructureSpecification(int quizId)
    {
        Query
            .Where(q => q.Id == quizId)

            .Include(q => q.Lesson)
                .ThenInclude(l => l.Teacher)

            .Include(q => q.Questions)
                .ThenInclude(ql => ql.Question)
                .ThenInclude(question => (question as MCQQuestion)!.Choices);
    }
}