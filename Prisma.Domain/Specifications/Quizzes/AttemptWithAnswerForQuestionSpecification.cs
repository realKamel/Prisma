using Ardalis.Specification;
using Prisma.Domain.Entities.QuizAggregate;

namespace Prisma.Domain.Specifications.Quizzes;

public class AttemptWithAnswerForQuestionSpecification : Specification<QuizAttempt>
{
    public AttemptWithAnswerForQuestionSpecification(int attemptId, Guid studentId, int questionId)
    {
        Query
            .Where(a => a.Id == attemptId && a.StudentId == studentId)
            .Include(a => a.Answers.Where(x => x.QuestionId == questionId));
    }
}