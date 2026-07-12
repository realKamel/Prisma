using Ardalis.Specification;
using Prisma.Domain.Entities.QuizAggregate;
using Prisma.Domain.Enums;

namespace Prisma.Domain.Specifications.QuizAttemptSpecs;

public class StudentWrittenQuestionsAttemptsSpec : Specification<QuizAttempt>
{
    public StudentWrittenQuestionsAttemptsSpec(Guid studentId)
    {
        Query
            .AsNoTracking()
            .Where(x => x.StudentId == studentId)
            .Include(x => x.Answers
                .Where(a => a.Question.Type == QuestionType.Written))
            .ThenInclude(x => x.Question);
    }
}