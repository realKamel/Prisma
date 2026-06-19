using Ardalis.Specification;
using Prisma.Domain.Entities.QuizAggregate;

namespace Prisma.Domain.Specifications.Quizzes;

public class StudentAttemptWithAnswersSpecification : Specification<QuizAttempt>
{
    public StudentAttemptWithAnswersSpecification(int quizId, Guid studentId)
    {
        Query
            .Where(a => a.QuizId == quizId && a.StudentId == studentId)
            .Include(a => a.Answers);
    }
}