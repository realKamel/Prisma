using System.Linq.Expressions;
using Ardalis.Specification;
using Prisma.Domain.Entities.LessonAggregate;

namespace Prisma.Domain.Specifications.Quizzes;

public sealed class AllAcademicYearsSpecification<TResult> : Specification<AcademicYear, TResult>
{
    public AllAcademicYearsSpecification(Expression<Func<AcademicYear, TResult>> expression)
    {
        Query.OrderBy(a => a.Id).AsNoTracking().Select(expression);
    }
}
