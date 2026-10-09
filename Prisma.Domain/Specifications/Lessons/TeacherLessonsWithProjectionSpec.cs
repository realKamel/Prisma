using System.Linq.Expressions;
using Ardalis.Specification;
using Prisma.Domain.Entities.LessonAggregate;

namespace Prisma.Domain.Specifications.Lessons;

public class TeacherLessonsWithProjectionSpec<TResult> : Specification<Lesson, TResult>
{
    public TeacherLessonsWithProjectionSpec(
        Guid teacherId,
        int pageNumber,
        int pageSize,
        Expression<Func<Lesson, TResult>> projection
    )
    {
        Query
            .Where(l => l.TeacherId == teacherId)
            .AsNoTrackingWithIdentityResolution()
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(projection);
    }
}
