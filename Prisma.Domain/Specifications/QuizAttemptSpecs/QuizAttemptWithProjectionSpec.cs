using System.Linq.Expressions;
using Ardalis.Specification;
using Prisma.Domain.Entities.QuizAggregate;
using Prisma.Domain.Enums;

namespace Prisma.Domain.Specifications.QuizAttemptSpecs;

public class QuizAttemptWithProjectionSpec<TResult> : Specification<QuizAttempt, TResult>
{
    public QuizAttemptWithProjectionSpec(
        Guid teacherId,
        DateTimeOffset from,
        DateTimeOffset? to,
        QuizAttemptStatus? status,
        Expression<Func<QuizAttempt, TResult>> projection)
    {
        Query
            .Where(a => a.SubmittedAt >= from
                     && (to == null || a.SubmittedAt < to)
                     && (status == null || a.Status == status)
                     && a.Quiz!.Lesson!.TeacherId == teacherId) 
            .AsNoTracking()
            .Select(projection);
    }
    public QuizAttemptWithProjectionSpec(Guid studentId, Expression<Func<QuizAttempt, TResult>> projection)
    {
        Query
            .Where(a => a.StudentId == studentId && a.Status == QuizAttemptStatus.Graded)
            .OrderByDescending(a => a.Degree)
            .Take(1)
            .AsNoTracking()
            .Select(projection);
    }
}