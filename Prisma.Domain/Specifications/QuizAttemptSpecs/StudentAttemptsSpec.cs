using System.Linq.Expressions;
using Ardalis.Specification;
using Prisma.Domain.Entities.QuizAggregate;
using Prisma.Domain.Enums;

namespace Prisma.Domain.Specifications.QuizAttemptSpecs;

public class StudentAttemptsSpec<TResult> : Specification<QuizAttempt, TResult>
{
    public StudentAttemptsSpec(Guid studentId, Expression<Func<QuizAttempt, TResult>> selector)
    {
        Query
            .AsNoTracking()
            .Where(x =>
                x.StudentId == studentId &&
                x.Status == QuizAttemptStatus.Submitted)
            .Include(x => x.Quiz)
            .Select(selector);
    }
}