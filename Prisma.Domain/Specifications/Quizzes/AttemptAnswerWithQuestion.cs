using System.Linq.Expressions;
using Ardalis.Specification;
using Prisma.Domain.Entities.QuizAggregate;
using Prisma.Domain.Enums;

namespace Prisma.Domain.Specifications.Quizzes;

public class AttemptAnswerWithQuestion<TResult> : Specification<AttemptAnswer, TResult>
{
    public AttemptAnswerWithQuestion(int id, Expression<Func<AttemptAnswer, TResult>> selector)
    {
        Query
            .Where(x => x.Id == id &&
                        x.Question.Type == QuestionType.Written)
            .AsNoTracking()
            .Include(x => x.Question)
            .Select(selector);
    }
}

public class AttemptAnswerWithQuestion : Specification<AttemptAnswer>
{
    public AttemptAnswerWithQuestion(int id)
    {
        Query
            .Where(x => x.Id == id && x.Question.Type == QuestionType.Written)
            .Include(x => x.Question);
    }
}